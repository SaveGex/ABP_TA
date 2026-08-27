using Application.Common.Interfaces;
using Application.Revenues.Queries.RevenueReport;
using Application.Revenues.Queries.RoomUtilizationReport;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UnitTests.Revenues.Queries
{
    public class RevenueAndUtilizationQueriesTests
    {
        private readonly Mock<IBookingDbContext> _contextMock;

        public RevenueAndUtilizationQueriesTests()
        {
            _contextMock = new Mock<IBookingDbContext>();
        }

        [Fact]
        public async Task GetRevenueReport_ShouldCalculateCorrectTotalAndBreakdown()
        {
            // Arrange
            var room = Room.Create("Hall A", 20, new Money(100, "USD"));
            var service = new Service(Guid.NewGuid(), "Projector", new Money(50, "USD"));

            var from = DateTime.UtcNow.Date;
            var to = from.AddDays(1);

            var slot = new TimeSlot(from.AddHours(10), from.AddHours(12)); // 2 hours = $200
            var booking = Booking.Create(room, slot, new List<Service> { service }); // + $50 = $250 total

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var bookingsDbSet = new List<Booking> { booking }.BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Bookings).Returns(bookingsDbSet.Object);

            var handler = new GetRevenueReportQueryHandler(_contextMock.Object);
            var query = new GetRevenueReportQuery(from, to);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalRevenue.Should().Be(250);
            result.RoomRevenue.Should().Be(200);
            result.ServicesRevenue.Should().Be(50);
            result.TotalBookings.Should().Be(1);
        }

        [Fact]
        public async Task GetRoomUtilizationReport_ShouldCalculateCorrectPercentages()
        {
            // Arrange
            var room = Room.Create("Hall B", 10, new Money(100, "USD"));
            var from = DateTime.UtcNow.Date;
            var to = from.AddHours(24); // 24 hours available

            var slot = new TimeSlot(from.AddHours(6), from.AddHours(12)); // 6 hours booked
            var booking = Booking.Create(room, slot, new List<Service>());

            var roomsDbSet = new List<Room> { room }.BuildMockDbSet();
            var bookingsDbSet = new List<Booking> { booking }.BuildMockDbSet();

            _contextMock.Setup(c => c.Rooms).Returns(roomsDbSet.Object);
            _contextMock.Setup(c => c.Bookings).Returns(bookingsDbSet.Object);

            var handler = new GetRoomUtilizationReportQueryHandler(_contextMock.Object);
            var query = new GetRoomUtilizationReportQuery(from, to);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.OverallUtilizationPercentage.Should().Be(25.00m); // 6 / 24 * 100 = 25%
            result.RoomDetails.Should().HaveCount(1);
            result.RoomDetails[0].TotalBookedHours.Should().Be(6);
        }
    }
}
