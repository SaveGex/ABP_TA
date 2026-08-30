using Application.Common.Interfaces;
using Domain.Services;
using Infrastructure.Configuration;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DI
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddInfrastructure(IConfiguration configuration)
            {
                services.AddSingleton<SoftDeleteInterceptor>();

                services.AddDbContext<BookingDbContext>((sp, options) =>
                {
                    var interceptor = sp.GetRequiredService<SoftDeleteInterceptor>();

                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                           .AddInterceptors([
                               interceptor,
                            ]);
                });

                services.AddScoped<IBookingDbContext>(provider =>
                    provider.GetRequiredService<BookingDbContext>());

                services.Configure<PricingOptions>(options =>
                    configuration.GetSection(PricingOptions.SectionName).Bind(options));

                services.AddTransient<IRentalPricingService, RentalPricingService>();


                return services;
            }
        }
    }
}
