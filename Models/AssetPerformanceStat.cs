namespace SmartPortfolioManager.Models
{
    public class AssetPerformanceStat
    {
        public string AssetName { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal InvestedAmount { get; set; }
        public decimal GainLoss { get; set; }
    }
}