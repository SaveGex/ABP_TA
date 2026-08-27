using Application.Common.Interfaces;
using Application.Rooms.Commands.CreateRoom;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Rooms.Commands
{
    public class CreateRoomCommandHandlerTests
    {
        private readonly Mock<IBookingDbContext> _contextMock;
        private readonly CreateRoomCommandHandler _handler;

        public CreateRoomCommandHandlerTests()
        {
            _contextMock = new Mock<IBookingDbContext>();
            _handler = new CreateRoomCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateRoomAndReturnResponseDto_WhenRequestIsValid()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var service = new Service(serviceId, "Wi-Fi", new Money(300, "UAH"));

            var servicesDbSet = new List<Service> { service }.BuildMockDbSet();
            var roomsDbSet = new List<Room>().BuildMockDbSet();

            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);
            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);

            var rate = new Money(2000, "UAH");
            var command = new CreateRoomCommand("Hall A", 50, rate, new List<Guid> { serviceId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be("Hall A");
            result.Capacity.Should().Be(50);
            result.BaseHourlyRate.Should().Be(rate);
            result.Services.Should().HaveCount(1);

            _contextMock.Verify(c => c.Rooms.Add(It.Is<Room>(r => r.Name == "Hall A")), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenServiceDoesNotExist()
        {
            // Arrange
            var servicesDbSet = new List<Service>().BuildMockDbSet();
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);

            var command = new CreateRoomCommand(
                "Hall B",
                30,
                new Money(1500, "UAH"),
                new List<Guid> { Guid.NewGuid() }
            );

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*do not exist*");

            _contextMock.Verify(c => c.Rooms.Add(It.IsAny<Room>()), Times.Never);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
