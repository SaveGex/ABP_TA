using Application.Common.Behaviors;
using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Application
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddApplication(IConfiguration configuration)
            {
                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(IBookingDbContext).Assembly);
                    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                });

                services.AddValidatorsFromAssembly(typeof(IBookingDbContext).Assembly);
                return services;
            }
        }

    }
}
