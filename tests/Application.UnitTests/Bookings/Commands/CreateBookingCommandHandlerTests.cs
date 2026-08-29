using Application.Bookings.Commands.CreateBooking;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Services;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Bookings.Commands
{
    /// <summary>
    /// Unit tests for <see cref="CreateBookingCommandHandler"/>.
    /// </summary>
    public class CreateBookingCommandHandlerTests
    {
        private readonly Mock<IBookingDbContext> _contextMock;
        private readonly Mock<IRentalPricingService> _pricingServiceMock;
        private readonly CreateBookingCommandHandler _handler;

        public CreateBookingCommandHandlerTests()
        {
            _contextMock = new Mock<IBookingDbContext>();
            _pricingServiceMock = new Mock<IRentalPricingService>();

            _handler = new CreateBookingCommandHandler(
                _contextMock.Object,
                _pricingServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateBooking_WhenRequestIsValid()
        {
            // Arrange
            var serviceId = Guid.NewGuid();

            var baseRate = new Money(100, "USD");
            var room = Room.Create("Conference Room A", 20, baseRate);
            var roomId = room.Id;
            room.AddService(serviceId);

            var service = new Service(serviceId, "Projector", new Money(30, "USD"));

            var slot = new TimeSlot(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3));

            // 2h * 100 = 200 USD
            var expectedCalculatedRoomCost = new Money(200m, "USD");
            _pricingServiceMock
                .Setup(p => p.CalculateRoomCost(room.BaseHourlyRate, slot))
                .Returns(expectedCalculatedRoomCost);

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var servicesDbSet = new List<Service> { service }.BuildMockDbSet();
            var bookingsDbSet = new List<Booking>().BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);
            _contextMock.Setup(c => c.Bookings).Returns(bookingsDbSet.Object);

            var command = new CreateBookingCommand(roomId, slot, new List<Guid> { serviceId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.RoomId.Should().Be(roomId);
            result.TotalPrice.Should().Be(230.0m);
            result.Services.Should().HaveCount(1);

            _pricingServiceMock.Verify(p => p.CalculateRoomCost(room.BaseHourlyRate, slot), Times.Once);
            _contextMock.Verify(c => c.Bookings.Add(It.IsAny<Booking>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
        {
            // Arrange
            var emptyRoomsDbSet = new List<Room>().BuildMockDbSet();
            _contextMock.Setup(c => c.Rooms).Returns(emptyRoomsDbSet.Object);

            var command = new CreateBookingCommand(
                Guid.NewGuid(),
                new TimeSlot(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)),
                new List<Guid>());

            // Act
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*was not found*");

            _pricingServiceMock.Verify(p => p.CalculateRoomCost(It.IsAny<Money>(), It.IsAny<TimeSlot>()), Times.Never);
            _contextMock.Verify(c => c.Bookings.Add(It.IsAny<Booking>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperationException_WhenSlotIsOccupied()
        {
            // Arrange
            var room = Room.Create("Room B", 10, new Money(50, "USD"));
            var roomId = room.Id;

            var existingSlot = new TimeSlot(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3));

            var existingBookingCost = new Money(100m, "USD");
            var existingBooking = Booking.Create(room, existingSlot, new List<Service>(), existingBookingCost);

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var bookingsDbSet = new List<Booking> { existingBooking }.BuildMockDbSet();
            var servicesDbSet = new List<Service>().BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Bookings).Returns(bookingsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);

            // Overlapping slot from 2 to 4 hours overlaps with existing 1-3
            var overlappingSlot = new TimeSlot(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(4));
            var command = new CreateBookingCommand(roomId, overlappingSlot, new List<Guid>());

            // Act
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already booked*");

            _contextMock.Verify(c => c.Bookings.Add(It.IsAny<Booking>()), Times.Never);
        }
    }
}
