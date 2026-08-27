namespace Domain.ValueObjects
{
    public record TimeSlot
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public TimeSlot(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("End must be after Start");

            Start = start;
            End = end;
        }

        public TimeSpan Duration => End - Start;

        /// <summary>
        /// Gets the duration of the slot in hours, rounded to 2 decimal places.
        /// </summary>
        public decimal DurationInHours =>
            Math.Round((decimal)(End - Start).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
        public bool Overlaps(TimeSlot other) => Start < other.End && other.Start < End;
    }
}
