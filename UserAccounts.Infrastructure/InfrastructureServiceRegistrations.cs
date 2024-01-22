namespace UserAccounts.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using UserAccounts.Domain.Interfaces;
    using UserAccounts.Infrastructure.Repositories;

    public static class InfrastructureServiceRegistrations
    {
        public static IServiceCollection AddInfrastructureServiceRegistrations(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("InMemoryDb"));

            services.AddTransient<IUserRepository, UserRepository>();
            return services;
        }
    }
}
