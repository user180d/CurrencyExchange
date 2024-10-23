using CurrencyExchange.Data;
using CurrencyExchange.Interfaces;
using CurrencyExchange.Services;
using System.Runtime.CompilerServices;

namespace CurrencyExchange.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services) 
        {
            // Add services to the container.
            services.AddControllers();
            // Register HttpClient for ApiService to call external APIs
            services.AddHttpClient<IApiService, ApiService>();
            // Register a background service for updating the currency database
            services.AddHostedService<CurrencyUpdateService>();

            // Add Swagger for API documentation and testing in development
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }
    }
}
