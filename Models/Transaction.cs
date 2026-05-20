using System.ComponentModel.DataAnnotations;
using SmartPortfolioManager.Models.Enums;

namespace SmartPortfolioManager.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Range(0.0001, 1000000)]
        public decimal Quantity { get; set; }

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public string? Notes { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid asset.")]
        public int AssetId { get; set; }

        public Asset? Asset { get; set; }
    }
}