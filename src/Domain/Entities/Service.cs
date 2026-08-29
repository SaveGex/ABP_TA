using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Service
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public Money Price { get; private set; } = null!;

        public Service(Guid id, string name, Money price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        private Service() { }
    }
}
