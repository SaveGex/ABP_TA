using Application.Bookings.Queries.GetBookingById;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Bookings.Queries
{
    public class GetBookingByIdQueryHandlerTests
    {
        private readonly Mock<IBookingDbContext> _dbContextMock;
        private readonly GetBookingByIdQueryHandler _sut; // System Under Test

        public GetBookingByIdQueryHandlerTests()
        {
            _dbContextMock = new Mock<IBookingDbContext>();
            _sut = new GetBookingByIdQueryHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_WhenBookingExists_ShouldReturnCorrectBookingResponseDto()
        {
            // Arrange
            var booking = CreateSampleBooking();
            SetupDbContextWithBookings(new[] { booking });

            var query = new GetBookingByIdQuery(booking.Id);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(booking.Id);
            result.RoomId.Should().Be(booking.RoomId);
            result.Start.Should().Be(booking.Slot.Start);
            result.End.Should().Be(booking.Slot.End);
            result.TotalPrice.Should().Be(booking.TotalPrice.Amount);
            result.Currency.Should().Be(booking.TotalPrice.Currency);
            result.Status.Should().Be(booking.Status.ToString());

            result.Services.Should().HaveCount(booking.Services.Count);

            var expectedFirstService = booking.Services.First();
            var actualFirstService = result.Services.First();

            actualFirstService.Id.Should().Be(expectedFirstService.ServiceId);
            actualFirstService.Name.Should().Be(expectedFirstService.Name);
            actualFirstService.Price.Should().Be(expectedFirstService.Price.Amount);
            actualFirstService.Currency.Should().Be(expectedFirstService.Price.Currency);
        }

        [Fact]
        public async Task Handle_WhenBookingDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            SetupDbContextWithBookings(Enumerable.Empty<Booking>());

            var query = new GetBookingByIdQuery(nonExistentId);

            // Act
            Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Booking with ID '{nonExistentId}' was not found.");
        }

        [Fact]
        public async Task Handle_WhenBookingHasNoServices_ShouldReturnEmptyServicesList()
        {
            // Arrange
            var bookingWithoutServices = new BookingBuilder()
                .WithServices(Enumerable.Empty<Service>())
                .Build();

            SetupDbContextWithBookings(new[] { bookingWithoutServices });

            var query = new GetBookingByIdQuery(bookingWithoutServices.Id);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Services.Should().BeEmpty();
        }

        #region Helpers & Test Data Builders

        private void SetupDbContextWithBookings(IEnumerable<Booking> bookings)
        {
            var mockDbSet = bookings.ToList().BuildMockDbSet();
            _dbContextMock.Setup(x => x.Bookings).Returns(mockDbSet.Object);
        }

        private static Booking CreateSampleBooking()
        {
            return new BookingBuilder().Build();
        }

        /// <summary>
        /// Builder pattern for creating <see cref="Booking"/> aggregate instances in tests.
        /// Encapsulates complex domain setups and ensures tests remain scalable when domain rules change.
        /// </summary>
        private class BookingBuilder
        {
            private Room _room = CreateDefaultRoom();
            private TimeSlot _slot = new(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(3));
            private Money _calculatedRoomCost = new(100.00m, "USD");
            private IEnumerable<Service> _services = new List<Service>
            {
                CreateDefaultService(Guid.NewGuid(), "Projector", 25.00m, "USD"),
                CreateDefaultService(Guid.NewGuid(), "Catering", 50.00m, "USD")
            };

            public BookingBuilder WithServices(IEnumerable<Service> services)
            {
                _services = services;
                return this;
            }

            public Booking Build()
            {
                return Booking.Create(_room, _slot, _services, _calculatedRoomCost);
            }

            private static Room CreateDefaultRoom()
            {
                return Room.Create("Conference Room", 10, new Money(50.00m, "USD"));
            }

            private static Service CreateDefaultService(Guid id, string name, decimal amount, string currency)
            {
                var price = new Money(amount, currency);
                var service = new Service(Guid.NewGuid(), name, price);

                // Override auto-generated ID if specific Guid is passed
                typeof(Service)
                    .GetProperty(nameof(Service.Id))?
                    .SetValue(service, id);

                return service;
            }
        }

        #endregion
    }
}
