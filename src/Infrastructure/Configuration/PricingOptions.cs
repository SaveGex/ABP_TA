namespace Infrastructure.Configuration
{
    public sealed class PricingOptions
    {
        public const string SectionName = "Pricing";

        public List<PricingRuleConfiguration> Rules { get; set; } = new();
    }

    public sealed class PricingRuleConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public decimal Multiplier { get; set; } = 1.0m;
        public int Priority { get; set; } = 0;
    }
}
