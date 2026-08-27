using Application.Common.Interfaces;
using Application.Rooms.Commands.DeleteRoom;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Rooms.Commands;

public class DeleteRoomCommandHandlerTests
{
    private readonly Mock<IBookingDbContext> _contextMock;
    private readonly DeleteRoomCommandHandler _handler;

    public DeleteRoomCommandHandlerTests()
    {
        _contextMock = new Mock<IBookingDbContext>();
        _handler = new DeleteRoomCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteRoom_WhenRoomExists()
    {
        // Arrange
        var room = Room.Create("Conference Room B", 15, new Money(100, "USD"));

        var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
        _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);

        var command = new DeleteRoomCommand(room.Id);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _contextMock.Verify(c => c.Rooms.Remove(It.Is<Room>(r => r.Id == room.Id)), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomsDbSet = new List<Room>().BuildMockDbSet();
        _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);

        var command = new DeleteRoomCommand(Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*was not found*");

        _contextMock.Verify(c => c.Rooms.Remove(It.IsAny<Room>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
