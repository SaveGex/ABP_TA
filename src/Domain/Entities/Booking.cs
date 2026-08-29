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
        public DateTime CreatedAt { get; private set; }

        private readonly List<BookedService> _services = new();
        public IReadOnlyCollection<BookedService> Services => _services.AsReadOnly();

        private Booking() { }

        /// <summary>
        /// Factory method to encapsulate creation logic and invariant validations for new bookings.
        /// </summary>
        /// <param name="room">The conference room being booked.</param>
        /// <param name="slot">The requested time slot for the reservation.</param>
        /// <param name="selectedServices">The list of optional services chosen by the user.</param>
        /// <param name="calculatedRoomCost">The room rental cost pre-calculated by the pricing service based on active time rules.</param>
        /// <returns>A fully initialized and validated <see cref="Booking"/> aggregate instance.</returns>
        public static Booking Create(
            Room room,
            TimeSlot slot,
            IEnumerable<Service> selectedServices,
            Money calculatedRoomCost)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                Slot = slot,
                Status = BookingStatus.Confirmed
            };

            var bookedServices = selectedServices
                .Select(service => new BookedService(service.Id, service.Name, service.Price))
                .ToList();

            booking._services.AddRange(bookedServices);

            var servicesCost = bookedServices.Sum(service => service.Price.Amount);

            booking.TotalPrice = new Money(
                calculatedRoomCost.Amount + servicesCost,
                calculatedRoomCost.Currency);

            return booking;
        }
    }
}
