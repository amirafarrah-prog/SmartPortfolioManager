using System.ComponentModel.DataAnnotations;

namespace SmartPortfolioManager.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [Range(0, 100000000)]
        public decimal InitialAmount { get; set; }

        [Range(0, 1000000)]
        public decimal MonthlyContribution { get; set; }

        public string? Notes { get; set; }
    }
}