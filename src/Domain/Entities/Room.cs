using Domain.Contracts;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Room : ISoftDelete
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public int Capacity { get; private set; }
        public Money BaseHourlyRate { get; private set; } = null!;
        private readonly List<Guid> _serviceIds = new();
        public IReadOnlyCollection<Guid> ServiceIds => _serviceIds.AsReadOnly();

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; private set; } = null;


        // Required by EF Core for materialization of entities with nested value objects
        private Room() { }

        public static Room Create(string name, int capacity, Money baseRate) =>
            new() { Id = Guid.NewGuid(), Name = name, Capacity = capacity, BaseHourlyRate = baseRate };

        public void UpdateRate(Money newRate) => BaseHourlyRate = newRate;
        public void AddService(Guid serviceId)
        {
            if (!_serviceIds.Contains(serviceId)) _serviceIds.Add(serviceId);
        }
        public void RemoveService(Guid serviceId) => _serviceIds.Remove(serviceId);

        public void Delete()
        {
            DeletedAt = DateTime.UtcNow;
        }
    }
}
