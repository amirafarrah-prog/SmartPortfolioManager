using System.ComponentModel.DataAnnotations;
using SmartPortfolioManager.Models.Enums;

namespace SmartPortfolioManager.Models
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = "";

        [Required]
        public AssetType Type { get; set; }

        [Range(0.01, 1000000)]
        public decimal CurrentPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Notes { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    }
}