using Application.Bookings.Commands.CreateBooking;
using Domain.ValueObjects;
using FluentAssertions;

namespace Application.UnitTests.Bookings.Commands
{
    public class CreateBookingCommandValidatorTests
    {
        private readonly CreateBookingCommandValidator _validator = new();

        [Fact]
        public void Validate_ShouldHaveError_WhenStartTimeIsInThePast()
        {
            // Arrange
            var pastSlot = new TimeSlot(DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(1));
            var command = new CreateBookingCommand(Guid.NewGuid(), pastSlot, new List<Guid>());

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Start"));
        }
    }
}
