using System.ComponentModel.DataAnnotations;

namespace SmartPortfolioManager.Models
{
    public class PriceHistory
    {
        [Key]
        public int Id { get; set; }

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue)]
        public int AssetId { get; set; }

        public Asset? Asset { get; set; }
    }
}