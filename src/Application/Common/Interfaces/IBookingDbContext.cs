using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces
{
    /// <summary>
    /// Database context contract exposed to the Application layer.
    /// </summary>
    public interface IBookingDbContext
    {
        DbSet<Room> Rooms { get; set; }
        DbSet<Booking> Bookings { get; set; }
        DbSet<Service> Services { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
