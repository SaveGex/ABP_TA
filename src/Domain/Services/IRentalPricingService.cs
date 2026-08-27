using Domain.ValueObjects;

namespace Domain.Servicess
{
    public interface IRentalPricingService
    {
        Money CalculateRoomCost(Money baseHourlyRate, TimeSlot slot);
    }

    // реалізація ділить слот на погодинні відрізки і застосовує коефіцієнт кожного
}
