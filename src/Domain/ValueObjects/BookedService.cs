namespace Domain.ValueObjects
{
    /// <summary>
    /// Represents a service snapshots bound to a specific booking.
    /// </summary>
    public record BookedService
    {
        public Guid ServiceId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public Money Price { get; private set; } = null!;

        // Required by EF Core for materialization of entities with nested value objects
#pragma warning disable CS8618
        private BookedService() { }
#pragma warning restore CS8618

        public BookedService(Guid serviceId, string name, Money price)
        {
            ServiceId = serviceId;
            Name = name;
            Price = price;
        }
    }
}
