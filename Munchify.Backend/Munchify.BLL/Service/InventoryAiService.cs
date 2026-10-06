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
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "Cảnh báo: Chưa cấu hình DeepSeek API Key. Đề xuất: Khuyến mãi giảm giá 50% hoặc thanh lý gấp lô hàng này.";
            }

            var daysLeft = (expiryDate.Date - DateTime.Now.Date).Days;
            var prompt = $"Hôm nay là {DateTime.Now:dd/MM/yyyy}. Món '{productName}' (còn {qty} {productName.Split(' ').LastOrDefault()}) sẽ HẾT HẠN vào {expiryDate:dd/MM/yyyy} (Chỉ còn {daysLeft} ngày). Tình trạng: RẤT KHẨN CẤP.";

            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = @"Bạn là quản lý cửa hàng tiện lợi. TUYỆT ĐỐI KHÔNG KHUYÊN: giảm giá, khuyến mãi, bán rẻ, dán nhãn hết hạn để bán, hay đem đi nấu ăn.
HÃY TRẢ VỀ KẾT QUẢ DƯỚI DẠNG ĐỊNH DẠNG JSON với 3 trường sau (không chứa markdown nào khác):
1. 'insight': Đoạn text đánh giá thiệt hại và tổng quan (ngắn gọn 1-2 câu).
2. 'cause': Nguyên nhân AI xác định (ví dụ: 'Nhập dư so với tốc độ bán thực tế...').
3. 'proposedAction': Hành động đề xuất (Chọn ĐÚNG 1 hành động tốt nhất trong 5 hành động: Tặng kèm miễn phí / Chia cho nhân viên / Trả nhà cung cấp / Giảm số lượng nhập / Tiêu hủy ngay)." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
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
                        .GetString();
                    
                    return aiText ?? "Không thể phân tích.";
                }
                
                return $"Lỗi khi gọi API AI (Status Code: {response.StatusCode}).";
            }
            catch (Exception)
            {
                return "Lỗi kết nối đến máy chủ AI.";
            }
        }
    }
}
