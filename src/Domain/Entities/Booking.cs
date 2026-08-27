using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a room booking aggregate within the domain.
    /// </summary>
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid RoomId { get; private set; }
        public TimeSlot Slot { get; private set; } = null!;
        public Money TotalPrice { get; private set; } = null!;
        public BookingStatus Status { get; private set; }

        private readonly List<BookedService> _services = new();
        public IReadOnlyCollection<BookedService> Services => _services.AsReadOnly();

        private Booking() { }

        /// <summary>
        /// Factory method to encapsulate creation logic and invariant validations for new bookings.
        /// </summary>
        public static Booking Create(
            Room room,
            TimeSlot slot,
            IEnumerable<Service> selectedServices)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                Slot = slot,
                Status = BookingStatus.Confirmed
            };

            var baseRoomCost = room.BaseHourlyRate.Amount * slot.DurationInHours;

            var bookedServices = selectedServices
                .Select(service => new BookedService(service.Id, service.Name, service.Price))
                .ToList();

            booking._services.AddRange(bookedServices);

            var servicesCost = bookedServices.Sum(service => service.Price.Amount);
            booking.TotalPrice = new Money(baseRoomCost + servicesCost, room.BaseHourlyRate.Currency);

            return booking;
        }
    }
}
