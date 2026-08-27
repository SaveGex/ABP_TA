using Application.Common.Interfaces;
using Application.Rooms.Commands.AddServiceToRoom;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Rooms.Commands
{
    public class AddServiceToRoomCommandHandlerTests
    {
        private readonly Mock<IBookingDbContext> _contextMock;
        private readonly AddServiceToRoomCommandHandler _handler;

        public AddServiceToRoomCommandHandlerTests()
        {
            _contextMock = new Mock<IBookingDbContext>();
            _handler = new AddServiceToRoomCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAddServiceToRoom_WhenRoomAndServiceExist()
        {
            // Arrange
            var room = Room.Create("Conference Room A", 20, new Money(100, "USD"));
            var serviceId = Guid.NewGuid();
            var service = new Service(serviceId, "Projector", new Money(50, "USD"));

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var servicesDbSet = new List<Service> { service }.BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);

            var command = new AddServiceToRoomCommand(room.Id, serviceId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            room.ServiceIds.Should().Contain(serviceId);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
        {
            // Arrange
            var roomsDbSet = new List<Room>().BuildMockDbSet();
            var servicesDbSet = new List<Service>().BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);

            var command = new AddServiceToRoomCommand(Guid.NewGuid(), Guid.NewGuid());

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Room with ID*was not found*");
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenServiceDoesNotExist()
        {
            // Arrange
            var room = Room.Create("Conference Room A", 20, new Money(100, "USD"));

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var servicesDbSet = new List<Service>().BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);

            var command = new AddServiceToRoomCommand(room.Id, Guid.NewGuid());

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Service with ID*was not found*");
        }
    }
}
