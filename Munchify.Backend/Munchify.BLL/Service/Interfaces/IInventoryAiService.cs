using System;
using System.Threading.Tasks;

namespace Munchify.BLL.Service.Interfaces
{
    public interface IInventoryAiService
    {
        Task<string> AnalyzeExpiryItemsAsync(string productName, int qty, DateTime expiryDate);
    }
}
