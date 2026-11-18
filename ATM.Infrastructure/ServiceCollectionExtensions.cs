using ATM.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ATM.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAtmPersistence(this IServiceCollection services)
    {
        services.AddDbContext<AtmDbContext>(options => options.UseInMemoryDatabase("ATM"));
        services.AddScoped<IAtmRepository, AtmRepository>();
        return services;
    }

    public static async Task InitializeAtmDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AtmDbContext>().Database.EnsureCreatedAsync();
    }
}
