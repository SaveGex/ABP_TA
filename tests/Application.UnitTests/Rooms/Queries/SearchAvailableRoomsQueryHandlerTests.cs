using Application.Common.Interfaces;
using Application.Rooms.Queries.SearchAvailableRooms;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Rooms.Queries
{
    public class SearchAvailableRoomsQueryHandlerTests
    {
        private readonly Mock<IBookingDbContext> _contextMock;
        private readonly SearchAvailableRoomsQueryHandler _handler;

        public SearchAvailableRoomsQueryHandlerTests()
        {
            _contextMock = new Mock<IBookingDbContext>();
            _handler = new SearchAvailableRoomsQueryHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOnlyAvailableRooms_MatchingCapacityAndNotBooked()
        {
            // Arrange
            var room1 = Room.Create("Room 1", 10, new Money(100, "UAH")); // Available & fits
            var room2 = Room.Create("Room 2", 5, new Money(100, "UAH"));  // Capacity too small
            var room3 = Room.Create("Room 3", 15, new Money(100, "UAH")); // Fits capacity but booked

            var targetDate = DateTime.Today.AddDays(1);
            var bookedSlot = new TimeSlot(targetDate.AddHours(10), targetDate.AddHours(12));
            var existingBooking = Booking.Create(room3, bookedSlot, new List<Service>());

            var roomsDbSet = new List<Room> { room1, room2, room3 }.BuildMockDbSet();
            var bookingsDbSet = new List<Booking> { existingBooking }.BuildMockDbSet();
            var servicesDbSet = new List<Service>().BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Bookings).Returns(bookingsDbSet.Object);
            _contextMock.Setup(c => c.Services).Returns(servicesDbSet.Object);

            var query = new SearchAvailableRoomsQuery(
                targetDate,
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(12),
                10
            );

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(room1.Id);
            result[0].Name.Should().Be("Room 1");
        }
    }
}
