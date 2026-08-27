using Application.Common.Interfaces;
using Application.Rooms.Commands.UpdateRoom;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Rooms.Commands
{
    public class UpdateRoomCommandHandlerTests
    {
        private readonly Mock<IBookingDbContext> _contextMock;
        private readonly UpdateRoomCommandHandler _handler;

        public UpdateRoomCommandHandlerTests()
        {
            _contextMock = new Mock<IBookingDbContext>();
            _handler = new UpdateRoomCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateRoomRate_WhenRoomExists()
        {
            // Arrange
            var room = Room.Create("Conference Hall A", 20, new Money(100, "USD"));
            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);

            var newRate = new Money(180, "USD");
            var command = new UpdateRoomCommand(room.Id, "Conference Hall A", 20, newRate);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            room.BaseHourlyRate.Should().Be(newRate);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
        {
            // Arrange
            var roomsDbSet = new List<Room>().BuildMockDbSet();
            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);

            var command = new UpdateRoomCommand(Guid.NewGuid(), "Non Existent", 10, new Money(100, "USD"));

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*was not found*");

            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
