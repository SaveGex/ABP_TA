using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="Room"/> entity.
/// </summary>
internal class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    /// <summary>
    /// Configures table properties, value objects, and encapsulation settings for rooms.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasQueryFilter(r => r.DeletedAt == null);

        builder.ToTable("Rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Capacity)
            .IsRequired();

        // Value Object mapping: BaseHourlyRate Money
        builder.ComplexProperty(r => r.BaseHourlyRate, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("BaseHourlyRateAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("BaseHourlyRateCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Primitive collection mapping backed by the private _serviceIds field
        builder.Property(r => r.ServiceIds)
            .HasField("_serviceIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(r => r.DeletedAt)
            .IsRequired(false);

    }
}