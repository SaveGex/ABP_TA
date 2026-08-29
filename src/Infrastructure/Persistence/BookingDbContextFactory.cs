using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating BookingDbContext instances during EF Core tooling operations.
/// </summary>
internal class AppDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        // Resolve path to MainWeb to reuse its configuration during migration operations
        var mainWebPath = Path.Combine(currentDirectory, "..", "MainWeb");
        var basePath = Directory.Exists(mainWebPath) ? mainWebPath : currentDirectory;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<BookingDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new BookingDbContext(optionsBuilder.Options);
    }
}