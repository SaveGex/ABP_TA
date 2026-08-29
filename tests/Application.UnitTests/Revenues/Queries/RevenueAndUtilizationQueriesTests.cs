using Application.Common.Interfaces;
using Application.Revenues.Queries.RoomUtilizationReport;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Revenues.Queries
{
    public class GetRoomUtilizationReportQueryHandlerTests
    {
        private readonly Mock<IBookingDbContext> _dbContextMock;
        private readonly GetRoomUtilizationReportQueryHandler _sut;

        public GetRoomUtilizationReportQueryHandlerTests()
        {
            _dbContextMock = new Mock<IBookingDbContext>();
            _sut = new GetRoomUtilizationReportQueryHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_WhenMultipleRoomsAndBookingsExist_ShouldCalculateUtilizationCorrectly()
        {
            // Arrange
            var from = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddHours(10);

            var room1 = new RoomBuilder().WithName("Conference Room A").Build();
            var room2 = new RoomBuilder().WithName("Executive Suite").Build();

            var booking1 = new BookingBuilder()
                .WithRoomId(room1.Id)
                .WithTimeSlot(from, from.AddHours(5))
                .WithStatus(BookingStatus.Confirmed)
                .Build();

            var booking2 = new BookingBuilder()
                .WithRoomId(room2.Id)
                .WithTimeSlot(from.AddHours(1), from.AddHours(3))
                .WithStatus(BookingStatus.Confirmed)
                .Build();

            SetupDbContext(new[] { room1, room2 }, new[] { booking1, booking2 });

            var query = new GetRoomUtilizationReportQuery(from, to, RoomId: null);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.From.Should().Be(from);
            result.To.Should().Be(to);

            result.OverallUtilizationPercentage.Should().Be(35.00m);
            result.RoomDetails.Should().HaveCount(2);

            var room1Detail = result.RoomDetails.First(r => r.RoomId == room1.Id);
            room1Detail.TotalBookedHours.Should().Be(5m);
            room1Detail.TotalAvailableHours.Should().Be(10m);
            room1Detail.UtilizationPercentage.Should().Be(50.00m);

            var room2Detail = result.RoomDetails.First(r => r.RoomId == room2.Id);
            room2Detail.TotalBookedHours.Should().Be(2m);
            room2Detail.TotalAvailableHours.Should().Be(10m);
            room2Detail.UtilizationPercentage.Should().Be(20.00m);
        }

        [Fact]
        public async Task Handle_WhenFilterByRoomId_ShouldCalculateUtilizationOnlyForSelectedRoom()
        {
            // Arrange
            var from = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddHours(10);

            var room1 = new RoomBuilder().Build();
            var room2 = new RoomBuilder().Build();

            var booking1 = new BookingBuilder()
                .WithRoomId(room1.Id)
                .WithTimeSlot(from, from.AddHours(4))
                .Build();

            var booking2 = new BookingBuilder()
                .WithRoomId(room2.Id)
                .WithTimeSlot(from, from.AddHours(8))
                .Build();

            SetupDbContext(new[] { room1, room2 }, new[] { booking1, booking2 });

            var query = new GetRoomUtilizationReportQuery(from, to, RoomId: room1.Id);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.RoomDetails.Should().HaveCount(1);

            var detail = result.RoomDetails.Single();
            detail.RoomId.Should().Be(room1.Id);
            detail.TotalBookedHours.Should().Be(4m);
            detail.UtilizationPercentage.Should().Be(40.00m);

            result.OverallUtilizationPercentage.Should().Be(40.00m);
        }

        [Fact]
        public async Task Handle_ShouldIgnoreCancelledBookings()
        {
            // Arrange
            var from = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddHours(10);

            var room = new RoomBuilder().Build();

            var activeBooking = new BookingBuilder()
                .WithRoomId(room.Id)
                .WithTimeSlot(from, from.AddHours(3))
                .WithStatus(BookingStatus.Confirmed)
                .Build();

            var cancelledBooking = new BookingBuilder()
                .WithRoomId(room.Id)
                .WithTimeSlot(from.AddHours(4), from.AddHours(8))
                .WithStatus(BookingStatus.Cancelled)
                .Build();

            SetupDbContext(new[] { room }, new[] { activeBooking, cancelledBooking });

            var query = new GetRoomUtilizationReportQuery(from, to, RoomId: null);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var detail = result.RoomDetails.Single();
            detail.TotalBookedHours.Should().Be(3m);
            detail.UtilizationPercentage.Should().Be(30.00m);
        }

        [Fact]
        public async Task Handle_WhenPeriodIsZero_ShouldReturnZeroUtilizationWithoutDividingByZero()
        {
            // Arrange
            var sameTime = DateTime.UtcNow;
            var room = new RoomBuilder().Build();

            SetupDbContext(new[] { room }, Enumerable.Empty<Booking>());

            var query = new GetRoomUtilizationReportQuery(sameTime, sameTime, RoomId: null);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.OverallUtilizationPercentage.Should().Be(0m);
            result.RoomDetails.Should().BeEmpty();
        }

        #region Helpers & Test Data Builders

        private void SetupDbContext(IEnumerable<Room> rooms, IEnumerable<Booking> bookings)
        {
            var mockRoomsDbSet = rooms.ToList().BuildMockDbSet();
            var mockBookingsDbSet = bookings.ToList().BuildMockDbSet();

            _dbContextMock.Setup(x => x.Rooms).Returns(mockRoomsDbSet.Object);
            _dbContextMock.Setup(x => x.Bookings).Returns(mockBookingsDbSet.Object);
        }

        /// <summary>
        /// Builder pattern for constructing <see cref="Room"/> domain entities in test environments.
        /// Uses domain creation logic to guarantee invariants and avoid Moq proxy issues with non-virtual members.
        /// </summary>
        private class RoomBuilder
        {
            private Guid _id = Guid.NewGuid();
            private string _name = "Conference Room";
            private int _capacity = 10;
            private Money _baseHourlyRate = new(50.00m, "USD");
            private readonly List<Guid> _serviceIds = new();

            public RoomBuilder WithId(Guid id)
            {
                _id = id;
                return this;
            }

            public RoomBuilder WithName(string name)
            {
                _name = name;
                return this;
            }

            public RoomBuilder WithCapacity(int capacity)
            {
                _capacity = capacity;
                return this;
            }

            public RoomBuilder WithServiceIds(IEnumerable<Guid> serviceIds)
            {
                _serviceIds.Clear();
                _serviceIds.AddRange(serviceIds);
                return this;
            }

            public Room Build()
            {
                var room = Room.Create(_name, _capacity, _baseHourlyRate);

                typeof(Room)
                    .GetProperty(nameof(Room.Id))?
                    .SetValue(room, _id);

                foreach (var serviceId in _serviceIds)
                {
                    room.AddService(serviceId);
                }

                return room;
            }
        }

        /// <summary>
        /// Builder pattern for constructing <see cref="Booking"/> domain aggregate instances.
        /// Uses concrete domain instances to maintain invariants across unit test scenarios.
        /// </summary>
        private class BookingBuilder
        {
            private Guid? _roomId;
            private TimeSlot _slot = new(DateTime.UtcNow, DateTime.UtcNow.AddHours(2));
            private BookingStatus _status = BookingStatus.Confirmed;
            private Money _cost = new(100.00m, "USD");

            public BookingBuilder WithRoomId(Guid roomId)
            {
                _roomId = roomId;
                return this;
            }

            public BookingBuilder WithTimeSlot(DateTime start, DateTime end)
            {
                _slot = new TimeSlot(start, end);
                return this;
            }

            public BookingBuilder WithStatus(BookingStatus status)
            {
                _status = status;
                return this;
            }

            public Booking Build()
            {
                var roomBuilder = new RoomBuilder();
                if (_roomId.HasValue)
                {
                    roomBuilder.WithId(_roomId.Value);
                }

                var room = roomBuilder.Build();

                var booking = Booking.Create(
                    room,
                    _slot,
                    Enumerable.Empty<Service>(),
                    _cost);

                if (_status != BookingStatus.Confirmed)
                {
                    typeof(Booking)
                        .GetProperty(nameof(Booking.Status))?
                        .SetValue(booking, _status);
                }

                return booking;
            }
        }

        #endregion
    }
}
