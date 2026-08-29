using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Configuration;
using Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.Services
{
    public class RentalPricingServiceTests
    {
        private readonly PricingOptions _defaultOptions = new()
        {
            Rules = new List<PricingRuleConfiguration>
        {
            new() { Name = "Standard hours", StartHour = 9, EndHour = 18, Multiplier = 1.0m },
            new() { Name = "Peak Hours Markup", StartHour = 12, EndHour = 14, Multiplier = 1.15m, Priority = 10 },
            new() { Name = "Evening Discount", StartHour = 18, EndHour = 23, Multiplier = 0.80m, Priority = 10 },
            new() { Name = "Morning Discount", StartHour = 6, EndHour = 9, Multiplier = 0.90m, Priority = 10 }
        }
        };

        private RentalPricingService CreateService(PricingOptions? options = null)
        {
            var optionsWrapper = Options.Create(options ?? _defaultOptions);
            return new RentalPricingService(optionsWrapper);
        }

        #region Simple Cases

        [Fact]
        public void CalculateRoomCost_SingleHourStandardTime_ReturnsBaseRate()
        {
            // Arrange
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 10, 0, 0),
                new DateTime(2026, 8, 31, 11, 0, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(100m);
            result.Currency.Should().Be("USD");
        }

        [Fact]
        public void CalculateRoomCost_SingleHourWithDiscount_AppliesDiscountMultiplier()
        {
            // Arrange (18:00 - 19:00 -> Evening Discount 0.80)
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 18, 0, 0),
                new DateTime(2026, 8, 31, 19, 0, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(80m);
        }

        [Fact]
        public void CalculateRoomCost_UnconfiguredHour_AppliesDefaultMultiplier()
        {
            // Arrange (02:00 - 04:00 -> standard multiplier 1.0)
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 2, 0, 0),
                new DateTime(2026, 8, 31, 4, 0, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(200m);
        }

        #endregion

        #region Complex Cases

        [Fact]
        public void CalculateRoomCost_OverlappingRules_PrefersHigherPriority()
        {
            // Arrange
            // Standard hours and Peak Hours Markup 
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 12, 0, 0),
                new DateTime(2026, 8, 31, 13, 0, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(115m);
        }

        [Fact]
        public void CalculateRoomCost_MultipleHoursWithDifferentMultipliers_CalculatesCorrectSum()
        {
            // Arrange
            // Slot 11:00 - 15:00:
            // 11:00 - 12:00 -> Standard (1.0) = 100
            // 12:00 - 13:00 -> Peak (1.15)    = 115
            // 13:00 - 14:00 -> Peak (1.15)    = 115
            // 14:00 - 15:00 -> Standard (1.0) = 100
            // Total: 100 + 115 + 115 + 100 = 430
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 11, 0, 0),
                new DateTime(2026, 8, 31, 15, 0, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(430m);
        }

        [Fact]
        public void CalculateRoomCost_SlotCrossingMidnight_CalculatesCorrectlyForBothDays()
        {
            // Arrange
            // Slot 22:00 - 02:00:
            // 22:00 - 23:00 -> Evening Discount (0.80) = 80
            // 23:00 - 00:00 -> Default (1.00)          = 100
            // 00:00 - 01:00 -> Default (1.00)          = 100
            // 01:00 - 02:00 -> Default (1.00)          = 100
            // Total: 80 + 100 + 100 + 100 = 380
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 22, 0, 0),
                new DateTime(2026, 9, 1, 2, 0, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(380m);
        }

        [Fact]
        public void CalculateRoomCost_EmptyRulesList_AppliesDefaultMultiplierForAllHours()
        {
            // Arrange
            var emptyOptions = new PricingOptions { Rules = new List<PricingRuleConfiguration>() };
            var service = CreateService(emptyOptions);
            var baseRate = new Money(50m, "EUR");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 10, 0, 0),
                new DateTime(2026, 8, 31, 13, 0, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(150m);
            result.Currency.Should().Be("EUR");
        }

        #endregion

        #region Fractional and Boundary Cases

        [Fact]
        public void CalculateRoomCost_PartialHourSlot_CalculatesProportionalCostWithPrecision()
        {
            // Arrange
            // 10:30 - 11:15 (45 minutes total = 0.75 hours) inside Standard hours (Multiplier 1.0)
            // Base rate 100 USD/hour -> 0.75 * 100 = 75 USD
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 10, 30, 0),
                new DateTime(2026, 8, 31, 11, 15, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(75.00m);
        }

        [Fact]
        public void CalculateRoomCost_PartialHourCrossingPeakAndStandard_SplitsProportionally()
        {
            // Arrange
            // 11:30 - 12:30 (60 minutes total)
            // 11:30 - 12:00 -> Standard hours (Multiplier 1.0): 30 mins = 0.5h * 100 = 50.00
            // 12:00 - 12:30 -> Peak Hours Markup (Multiplier 1.15, Priority 10): 30 mins = 0.5h * 115 = 57.50
            // Total: 50.00 + 57.50 = 107.50
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 11, 30, 0),
                new DateTime(2026, 8, 31, 12, 30, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(107.50m);
        }

        [Fact]
        public void CalculateRoomCost_BoundaryAtRuleEdge_ExclusivityBehaviorHandledCorrectly()
        {
            // Arrange
            // Standard hours: 9 to 18 (exclusive of 18)
            // Evening Discount: 18 to 23
            // Slot ending precisely at 18:00 (17:00 - 18:00) should fall fully under Standard hours, 
            // whereas a slot starting at 18:00 (18:00 - 19:00) falls under Evening Discount.
            var service = CreateService();
            var baseRate = new Money(100m, "USD");

            var slotBeforeBoundary = new TimeSlot(
                new DateTime(2026, 8, 31, 17, 0, 0),
                new DateTime(2026, 8, 31, 18, 0, 0)
            );

            var slotAtBoundary = new TimeSlot(
                new DateTime(2026, 8, 31, 18, 0, 0),
                new DateTime(2026, 8, 31, 19, 0, 0)
            );

            // Act
            var resultBefore = service.CalculateRoomCost(baseRate, slotBeforeBoundary);
            var resultAt = service.CalculateRoomCost(baseRate, slotAtBoundary);

            // Assert
            resultBefore.Amount.Should().Be(100m); // Standard (1.0)
            resultAt.Amount.Should().Be(80m);      // Evening Discount (0.80)
        }

        [Fact]
        public void CalculateRoomCost_ComplexOverlappingPrioritiesAcrossMultipleHours_AppliesHighestPriorityWithPrecision()
        {
            // Arrange
            // Slot spanning 11:45 to 14:15 (2.5 hours):
            // - 11:45 - 12:00 (15m): Standard hours (1.0) -> 0.25 * 100 = 25.00
            // - 12:00 - 14:00 (120m): Peak Hours Markup (1.15, Priority 10) vs Standard (1.0). Peak wins -> 2.0 * 115 = 230.00
            // - 14:00 - 14:15 (15m): Standard hours (1.0) -> 0.25 * 100 = 25.00
            // Total: 25.00 + 230.00 + 25.00 = 280.00
            var service = CreateService();
            var baseRate = new Money(100m, "USD");
            var slot = new TimeSlot(
                new DateTime(2026, 8, 31, 11, 45, 0),
                new DateTime(2026, 8, 31, 14, 15, 0)
            );

            // Act
            var result = service.CalculateRoomCost(baseRate, slot);

            // Assert
            result.Amount.Should().Be(280.00m);
        }

        #endregion

    }
}
