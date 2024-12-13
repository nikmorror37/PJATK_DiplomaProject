using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BookingWepApp.Data;
using System;

namespace BookingWepApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build the host
            var host = CreateHostBuilder(args).Build();

            // Create a scope for dependency injection
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Get the MongoDbContext from the DI container
                    var mongoDbContext = services.GetRequiredService<MongoDbContext>();

                    // Seed initial data into MongoDB
                    mongoDbContext.SeedData();
                }
                catch (Exception ex)
                {
                    // Log any errors during MongoDB initialization
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during MongoDB initialization.");
                }
            }

            // Run the host
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Specify the Startup class for configuration
                    webBuilder.UseStartup<Startup>();
                });
    }
}

