using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Munchify.BLL.Service.Interfaces;

namespace Munchify.BLL.Service
{
    public class InventoryAiService : IInventoryAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public InventoryAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["DeepSeek:ApiKey"] ?? string.Empty;
        }

        public async Task<string> AnalyzeExpiryItemsAsync(string productName, int qty, DateTime expiryDate)
        {
            return await AnalyzeRecordAsync(productName, qty, "Món", "EXPIRED", null, expiryDate);
        }

        public async Task<string> AnalyzeRecordAsync(string productName, int qty, string unitName, string reportType, string? reason, DateTime? expiryDate)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return JsonSerializer.Serialize(new
                {
                    insight = "Chưa cấu hình DeepSeek API Key trong hệ thống.",
                    cause = "Thiếu cấu hình API Key.",
                    proposedAction = "Tiêu hủy ngay"
                });
            }

            string situationPrompt;
            if (string.Equals(reportType, "DAMAGED", StringComparison.OrdinalIgnoreCase))
            {
                situationPrompt = $"Sản phẩm '{productName}' (Số lượng: {qty} {unitName}) gặp sự cố HƯ HỎNG/THIỆT HẠI. Lý do báo cáo: '{(string.IsNullOrWhiteSpace(reason) ? "Không có lý do chi tiết" : reason)}'.";
            }
            else
            {
                var daysLeft = expiryDate.HasValue ? (expiryDate.Value.Date - DateTime.Now.Date).Days : 0;
                var expiryStr = expiryDate.HasValue ? expiryDate.Value.ToString("dd/MM/yyyy") : "Đã quá hạn sử dụng";
                string timeContext = daysLeft > 0 ? $"còn {daysLeft} ngày là hết hạn" : (daysLeft == 0 ? "hết hạn hôm nay" : $"đã quá hạn {Math.Abs(daysLeft)} ngày");
                situationPrompt = $"Món '{productName}' (Số lượng: {qty} {unitName}) vào ngày hết hạn {expiryStr} ({timeContext}). Tình trạng: '{(string.IsNullOrWhiteSpace(reason) ? "Hết hạn / Cận hạn sử dụng" : reason)}'. Số ngày còn lại: {daysLeft} ngày.";
            }

            var prompt = $"Hôm nay là {DateTime.Now:dd/MM/yyyy}. {situationPrompt} Hãy đánh giá tình hình thiệt hại, nguyên nhân và đưa ra hành động tối ưu theo đúng quy tắc nghiệp vụ.";

            var requestBody = new
            {
                model = "deepseek-chat",
                temperature = 0.2,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new 
                    { 
                        role = "system", 
                        content = @"Bạn là chuyên gia quản lý kho và xử lý hao hụt của chuỗi cửa hàng tiện lợi / F&B Munchify POS.
Hãy đánh giá trường hợp hàng hỏng / cận hạn / hết hạn và đưa ra đề xuất xử lý tối ưu.

QUY TẮC BẮT BUỘC ĐỂ CHỌN 'proposedAction':
1. NẾU LÀ HÀNG HƯ HỎNG, ẨM MỐC HOẶC ĐÃ QUÁ HẠN (daysLeft <= 0):
   -> 'proposedAction': Bắt buộc là 'Tiêu hủy ngay'
2. NẾU LÀ HÀNG CẬN HẠN (dựa vào số ngày còn lại 'daysLeft'):
   - Dưới 5 ngày (daysLeft < 5): 'proposedAction': Bắt buộc là 'Tiêu hủy ngay' (vì không an toàn để đưa cho khách hàng)
   - Từ 5 đến 10 ngày (5 <= daysLeft <= 10): 'proposedAction': 'Tặng kèm khách mua hàng' (làm quà tặng kích cầu mua hàng để xả tồn nhanh)
   - Trên 10 ngày (daysLeft > 10): 'proposedAction': 'Chia cho nhân viên' hoặc 'Giảm số lượng nhập đợt sau'

YÊU CẦU BẮT BUỘC: Chỉ trả về DUY NHẤT một chuỗi JSON hợp lệ (KHÔNG có markdown ```json, không có text chào hỏi hay giải thích bên ngoài).
Cấu trúc JSON đúng 3 trường:
{
  ""insight"": ""1-2 câu nhận định ngắn gọn về mức độ thiệt hại và rủi ro nếu giữ hàng"",
  ""cause"": ""Nguyên nhân chính dẫn đến tình trạng này (ngắn gọn dưới 20 từ, ví dụ: 'Bảo quản sai độ ẩm/nhiệt độ', 'Nhập hàng vượt quá nhu cầu tiêu thụ', 'Bao bì bị rách trong vận chuyển'...)"",
  ""proposedAction"": ""Chính xác tên 1 hành động theo đúng quy tắc số ngày ở trên""
}" 
                    },
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            try
            {
                var response = await _httpClient.PostAsync("https://api.deepseek.com/chat/completions", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var aiText = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "";

                    var cleaned = aiText.Trim();
                    if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                    {
                        cleaned = cleaned.Substring(7);
                    }
                    else if (cleaned.StartsWith("```"))
                    {
                        cleaned = cleaned.Substring(3);
                    }
                    if (cleaned.EndsWith("```"))
                    {
                        cleaned = cleaned.Substring(0, cleaned.Length - 3);
                    }
                    cleaned = cleaned.Trim();

                    // Verify valid JSON
                    try
                    {
                        using var testDoc = JsonDocument.Parse(cleaned);
                        return cleaned;
                    }
                    catch
                    {
                        return JsonSerializer.Serialize(new
                        {
                            insight = cleaned,
                            cause = "Dữ liệu AI ghi nhận",
                            proposedAction = "Tiêu hủy ngay"
                        });
                    }
                }
                
                return JsonSerializer.Serialize(new
                {
                    insight = $"Lỗi khi kết nối dịch vụ AI (Status Code: {response.StatusCode}).",
                    cause = "Dịch vụ AI tạm thời không khả dụng.",
                    proposedAction = "Kiểm tra lại sau"
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new
                {
                    insight = "Lỗi kết nối máy chủ AI: " + ex.Message,
                    cause = "Lỗi đường truyền hoặc máy chủ.",
                    proposedAction = "Kiểm tra kết nối mạng"
                });
            }
        }
    }
}
