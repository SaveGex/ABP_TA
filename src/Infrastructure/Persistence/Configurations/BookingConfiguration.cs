using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="Booking"/> entity.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <summary>
    /// Configures the database schema mapping, relationships, and value objects for bookings.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        // Foreign key relationship to Room entity without navigation property back
        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Value Object mapping: TimeSlot
        builder.OwnsOne(b => b.Slot, slot =>
        {
            slot.Property(s => s.Start)
                .HasColumnName("StartTime")
                .IsRequired();

            slot.Property(s => s.End)
                .HasColumnName("EndTime")
                .IsRequired();
        });

        // Value Object mapping: TotalPrice Money
        builder.OwnsOne(b => b.TotalPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalPriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TotalPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Enum persisted as string to ensure schema durability against enum value shifts
        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Owned collection mapping for BookedService Value Objects into a separate table
        builder.OwnsMany(b => b.Services, bs =>
        {
            bs.ToTable("BookingServices");
            bs.WithOwner().HasForeignKey("BookingId");

            bs.Property(s => s.Name)
                .HasMaxLength(100);

            bs.OwnsOne(s => s.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasPrecision(18, 2);
                money.Property(m => m.Currency)
                    .HasMaxLength(3);
            });
        });

        // Configures EF Core to access the private _services collection field directly
        builder.Navigation(b => b.Services)
            .HasField("_services")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}