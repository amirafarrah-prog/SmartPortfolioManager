using SmartPortfolioManager.Models;

namespace SmartPortfolioManager.Services
{
    public interface IPortfolioService
    {
        Task<List<Asset>> GetAssetsAsync();
        Task<List<Asset>> SearchAssetsAsync(string? searchText, string? assetType);
        Task<Asset?> GetAssetByIdAsync(int id);
        Task AddAssetAsync(Asset asset);
        Task UpdateAssetAsync(Asset asset);
        Task DeleteAssetAsync(int id);

        Task<List<Transaction>> GetTransactionsAsync();
        Task<List<Transaction>> SearchTransactionsAsync(string? searchText, string? transactionType, int? assetId);
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task AddTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(int id);

        Task<Budget?> GetBudgetAsync();
        Task SaveBudgetAsync(Budget budget);
        Task DeleteBudgetAsync();

        Task<int> GetTotalAssetsAsync();
        Task<decimal> GetCurrentPortfolioValueAsync();
        Task<decimal> GetTotalInvestedAsync();
        Task<decimal> GetOverallGainLossAsync();

        Task<List<AssetPerformanceStat>> GetPerformanceByAssetAsync();
        Task<List<AssetTypeAllocationStat>> GetAllocationByAssetTypeAsync();
    }
}