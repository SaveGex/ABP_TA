using Domain.Services;
using Domain.ValueObjects;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    internal sealed class RentalPricingService : IRentalPricingService
    {
        private readonly PricingOptions _options;
        private const decimal StandardMultiplier = 1.0m;
        private readonly decimal[] _hourlyMultipliers = new decimal[24];

        public RentalPricingService(IOptions<PricingOptions> options)
        {
            _options = options.Value;
            BuildHourlyMultiplierCache();
        }

        public Money CalculateRoomCost(Money baseHourlyRate, TimeSlot slot)
        {
            if (slot.Start >= slot.End)
                return new Money(0, baseHourlyRate.Currency);

            decimal totalCost = 0;

            // Offset from the start of the day in minutes for start/end
            // We work with full days and fractions of days
            var currentStart = slot.Start;

            while (currentStart < slot.End)
            {
                // The boundary between the current day and the next (24:00/00:00)
                var endOfDay = currentStart.Date.AddDays(1);
                var currentEnd = slot.End < endOfDay ? slot.End : endOfDay;

                // We calculate the cost over a 24-hour period by intersecting intervals
                totalCost += CalculateDailyCost(baseHourlyRate.Amount, currentStart, currentEnd);

                currentStart = currentEnd;
            }

            return new Money(totalCost, baseHourlyRate.Currency);
        }

        private decimal CalculateDailyCost(decimal baseRate, DateTime start, DateTime end)
        {
            // Convert the start and end times to minutes from the start of the day [0..1440]
            double startMinute = start.TimeOfDay.TotalMinutes;
            double endMinute = end.TimeOfDay.TotalMinutes;

            // If "end" corresponds to midnight the following day (00:00), that is 1,440 minutes
            if (end.Date > start.Date && endMinute == 0)
            {
                endMinute = 1440;
            }

            decimal dayCost = 0;

            // We only iterate through those 24-hour intervals that intersect with the slot
            int startHour = (int)(startMinute / 60);
            int endHour = (int)Math.Ceiling(endMinute / 60);

            for (int hour = startHour; hour < endHour; hour++)
            {
                // The boundaries of the current hour in minutes from the start of the day
                double hourStartMinute = hour * 60;
                double hourEndMinute = (hour + 1) * 60;

                // The intersection of two intervals: [startMinute, endMinute] and [hourStartMinute, hourEndMinute]
                double overlapStart = Math.Max(startMinute, hourStartMinute);
                double overlapEnd = Math.Min(endMinute, hourEndMinute);

                if (overlapStart < overlapEnd)
                {
                    double durationInHours = (overlapEnd - overlapStart) / 60.0;
                    decimal multiplier = _hourlyMultipliers[hour];

                    dayCost += baseRate * multiplier * (decimal)durationInHours;
                }
            }

            return dayCost;
        }

        /// <summary>
        /// Pre-calculates the aggregate multiplier for each of the 24 hours considering priorities.
        /// Executed once during service initialization.
        /// </summary>
        private void BuildHourlyMultiplierCache()
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var matchedRule = _options.Rules
                    .Where(rule => hour >= rule.StartHour && hour < rule.EndHour)
                    .OrderByDescending(rule => rule.Priority)
                    .FirstOrDefault();

                _hourlyMultipliers[hour] = matchedRule?.Multiplier ?? StandardMultiplier;
            }
        }
    }
}
