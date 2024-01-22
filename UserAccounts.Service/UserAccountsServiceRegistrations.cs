namespace UserAccounts.Service
{
    using AutoMapper;
    using FluentValidation;
    using Microsoft.Extensions.DependencyInjection;
    
    using UserAccounts.Service.Mappers;
    using UserAccounts.Service.Models;
    using UserAccounts.Service.Services;
    using UserAccounts.Service.Validations;

    public static class UserAccountsServiceRegistrations
    {
        public static IServiceCollection AddUserAccountsServiceRegistrations(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(UserAccountProfile));
            services.AddTransient<IUserService, UserService>();
            services.AddScoped<IValidator<UserModel>, UserAccountValidator>();
            return services;
        }
    }
}
