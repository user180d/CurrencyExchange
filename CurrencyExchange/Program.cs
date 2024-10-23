 using CurrencyExchange.Data;
using Microsoft.EntityFrameworkCore;
using CurrencyExchange.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
        .AddServices()
        .AddDbContext<DataContext>(options =>  // Register the DataContext with SQL Server
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
}
var app = builder.Build();
{

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Ensure HTTPS redirection for security
    app.UseHttpsRedirection();

    // Enable Authorization middleware
    app.UseAuthorization();

    // Map controller routes
    app.MapControllers();

    try
    {
        // Run the application
        app.Run();
    }
    catch (Exception ex)
    {
        // Log any startup exceptions (you can add more advanced logging later)
        Console.WriteLine($"Exception occurred during startup: {ex.Message}");
    }
}

