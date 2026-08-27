using Application.Bookings.Commands;
using Application.Common.Interfaces;
using Domain.Entities;
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
        private readonly CreateBookingCommandHandler _handler;

        public CreateBookingCommandHandlerTests()
        {
            _contextMock = new Mock<IBookingDbContext>();
            _handler = new CreateBookingCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateBooking_WhenRequestIsValid()
        {
            // Arrange
            var serviceId = Guid.NewGuid();

            var room = Room.Create("Conference Room A", 20, new Money(100, "USD"));
            var roomId = room.Id;
            room.AddService(serviceId);

            var service = new Service(serviceId, "Projector", new Money(30, "USD"));

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var servicesDbSet = new List<Service> { service }.BuildMockDbSet();
            var bookingsDbSet = new List<Booking>().BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);
            _contextMock.Setup(c => c.Bookings).Returns(bookingsDbSet.Object);

            var slot = new TimeSlot(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3));
            var command = new CreateBookingCommand(roomId, slot, new List<Guid> { serviceId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.RoomId.Should().Be(roomId);
            result.TotalPrice.Should().Be(230.0m); // (2 hours * 100) + 30 service
            result.Services.Should().HaveCount(1);

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
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperationException_WhenSlotIsOccupied()
        {
            // Arrange
            var room = Room.Create("Room B", 10, new Money(50, "USD"));
            var roomId = room.Id;

            var existingSlot = new TimeSlot(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3));
            var existingBooking = Booking.Create(room, existingSlot, new List<Service>());

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var bookingsDbSet = new List<Booking> { existingBooking }.BuildMockDbSet();
            var servicesDbSet = new List<Service>().BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Bookings).Returns(bookingsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);

            // Overlapping slot
            var overlappingSlot = new TimeSlot(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(4));
            var command = new CreateBookingCommand(roomId, overlappingSlot, new List<Guid>());

            // Act
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already booked*");
        }
    }
}
