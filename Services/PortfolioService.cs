using Microsoft.EntityFrameworkCore;
using SmartPortfolioManager.Data;
using SmartPortfolioManager.Models;
using SmartPortfolioManager.Models.Enums;

namespace SmartPortfolioManager.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly AppDbContext _context;

        public PortfolioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Asset>> GetAssetsAsync()
        {
            return await _context.Assets
                .Include(a => a.Transactions)
                .Include(a => a.PriceHistories)
                .ToListAsync();
        }

        public async Task<List<Asset>> SearchAssetsAsync(string? searchText, string? assetType)
        {
            IQueryable<Asset> query = _context.Assets
                .Include(a => a.Transactions)
                .Include(a => a.PriceHistories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(a =>
                    a.Name.Contains(searchText) ||
                    a.Symbol.Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(assetType))
            {
                query = query.Where(a => a.Type.ToString() == assetType);
            }

            return await query.ToListAsync();
        }

        public async Task<Asset?> GetAssetByIdAsync(int id)
        {
            return await _context.Assets.FindAsync(id);
        }

        public async Task AddAssetAsync(Asset asset)
        {
            asset.CreatedAt = DateTime.Now;

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAssetAsync(Asset asset)
        {
            _context.Assets.Update(asset);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAssetAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);

            if (asset != null)
            {
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Asset)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<Transaction>> SearchTransactionsAsync(string? searchText, string? transactionType, int? assetId)
        {
            IQueryable<Transaction> query = _context.Transactions
                .Include(t => t.Asset)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(t =>
                    t.Asset != null &&
                    (
                        t.Asset.Name.Contains(searchText) ||
                        t.Asset.Symbol.Contains(searchText)
                    ));
            }

            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                query = query.Where(t => t.Type.ToString() == transactionType);
            }

            if (assetId.HasValue && assetId.Value > 0)
            {
                query = query.Where(t => t.AssetId == assetId.Value);
            }

            return await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            transaction.TransactionDate = DateTime.Now;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Budget?> GetBudgetAsync()
        {
            return await _context.Budgets.FirstOrDefaultAsync();
        }

        public async Task SaveBudgetAsync(Budget budget)
        {
            var existingBudget = await _context.Budgets.FirstOrDefaultAsync();

            if (existingBudget == null)
            {
                _context.Budgets.Add(budget);
            }
            else
            {
                existingBudget.InitialAmount = budget.InitialAmount;
                existingBudget.MonthlyContribution = budget.MonthlyContribution;
                existingBudget.Notes = budget.Notes;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBudgetAsync()
        {
            var existingBudget = await _context.Budgets.FirstOrDefaultAsync();

            if (existingBudget != null)
            {
                _context.Budgets.Remove(existingBudget);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalAssetsAsync()
        {
            return await _context.Assets.CountAsync();
        }

        public async Task<decimal> GetCurrentPortfolioValueAsync()
        {
            var assets = await GetAssetsAsync();

            return assets.Sum(a => GetCurrentQuantity(a) * a.CurrentPrice);
        }

        public async Task<decimal> GetTotalInvestedAsync()
        {
            return await _context.Transactions
                .Where(t => t.Type == TransactionType.Buy)
                .SumAsync(t => t.Quantity * t.Price);
        }

        public async Task<decimal> GetOverallGainLossAsync()
        {
            var currentValue = await GetCurrentPortfolioValueAsync();
            var invested = await GetTotalInvestedAsync();

            return currentValue - invested;
        }

        public async Task<List<AssetPerformanceStat>> GetPerformanceByAssetAsync()
        {
            var assets = await GetAssetsAsync();

            return assets.Select(a => new AssetPerformanceStat
            {
                AssetName = a.Name,
                Quantity = GetCurrentQuantity(a),
                CurrentValue = GetCurrentQuantity(a) * a.CurrentPrice,
                InvestedAmount = GetInvestedAmount(a),
                GainLoss = (GetCurrentQuantity(a) * a.CurrentPrice) - GetInvestedAmount(a)
            }).ToList();
        }

        public async Task<List<AssetTypeAllocationStat>> GetAllocationByAssetTypeAsync()
        {
            var assets = await GetAssetsAsync();

            return assets
                .GroupBy(a => a.Type)
                .Select(g => new AssetTypeAllocationStat
                {
                    TypeName = g.Key.ToString(),
                    TotalValue = g.Sum(a => GetCurrentQuantity(a) * a.CurrentPrice)
                })
                .ToList();
        }

        private decimal GetCurrentQuantity(Asset asset)
        {
            var boughtQuantity = asset.Transactions
                .Where(t => t.Type == TransactionType.Buy)
                .Sum(t => t.Quantity);

            var soldQuantity = asset.Transactions
                .Where(t => t.Type == TransactionType.Sell)
                .Sum(t => t.Quantity);

            return boughtQuantity - soldQuantity;
        }

        private decimal GetInvestedAmount(Asset asset)
        {
            return asset.Transactions
                .Where(t => t.Type == TransactionType.Buy)
                .Sum(t => t.Quantity * t.Price);
        }
    }
}