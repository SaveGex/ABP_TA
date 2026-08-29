using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeders
{
    public static class DbSpecificationInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<BookingDbContext>();

            await context.Database.EnsureCreatedAsync();

            if (context.Rooms.Any())
            {
                return;
            }

            // 1. Initial Services from Spec
            var projector = new Service(Guid.NewGuid(), "Projector", new Money(500, "UAH"));
            var wifi = new Service(Guid.NewGuid(), "Wi-Fi", new Money(300, "UAH"));
            var sound = new Service(Guid.NewGuid(), "Sound System", new Money(700, "UAH"));

            await context.Services.AddRangeAsync(projector, wifi, sound);

            // 2. Initial Rooms from Spec
            var roomA = Room.Create("Hall A", 50, new Money(2000, "UAH"));
            var roomB = Room.Create("Hall B", 100, new Money(3500, "UAH"));
            var roomC = Room.Create("Hall C", 30, new Money(1500, "UAH"));

            // Assign initial services
            roomA.AddService(projector.Id);
            roomA.AddService(wifi.Id);

            roomB.AddService(projector.Id);
            roomB.AddService(wifi.Id);
            roomB.AddService(sound.Id);

            await context.Rooms.AddRangeAsync(roomA, roomB, roomC);
            await context.SaveChangesAsync();
        }
    }
}
