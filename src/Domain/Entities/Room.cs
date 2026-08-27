using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Room
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Capacity { get; private set; }
        public Money BaseHourlyRate { get; private set; }
        private readonly List<Guid> _serviceIds = new();
        public IReadOnlyCollection<Guid> ServiceIds => _serviceIds.AsReadOnly();

        // Required by EF Core for materialization of entities with nested value objects
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Room() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public static Room Create(string name, int capacity, Money baseRate) =>
            new() { Id = Guid.NewGuid(), Name = name, Capacity = capacity, BaseHourlyRate = baseRate };

        public void UpdateRate(Money newRate) => BaseHourlyRate = newRate;
        public void AddService(Guid serviceId)
        {
            if (!_serviceIds.Contains(serviceId)) _serviceIds.Add(serviceId);
        }
        public void RemoveService(Guid serviceId) => _serviceIds.Remove(serviceId);
    }
}
