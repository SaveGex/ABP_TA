using Domain.ValueObjects;

namespace Domain.Services
{
    public interface IRentalPricingService
    {
        Money CalculateRoomCost(Money baseHourlyRate, TimeSlot slot);
    }
}
