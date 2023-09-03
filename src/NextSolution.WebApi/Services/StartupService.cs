using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Constants;
using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using NextSolution.Core.Services;
using NextSolution.Infrastructure.Data;
using System;

namespace NextSolution.WebApi.Services
{
    public class StartupService : IHostedService
    {
        private IServiceProvider _serviceProvider;
        public StartupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILogger<StartupService>>();
            var environment = services.GetRequiredService<IWebHostEnvironment>();

            if (environment.IsDevelopment())
            {
                // Ensure database is created.
                var dbContext = services.GetRequiredService<AppDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            try
            {
                logger.LogInformation("Start seeding the database.");

                var roleRepository = services.GetRequiredService<IRoleRepository>();

                if (!(await roleRepository.AnyAsync()))
                {
                    foreach (var roleName in Roles.All)
                    {
                        await roleRepository.CreateAsync(new Role(roleName));
                    }
                }

                logger.LogInformation("Finished seeding the database.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }

            try
            {
                var clientRepository = services.GetRequiredService<IClientRepository>();
                await clientRepository.DeactivateAllAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while disconnecting all clients.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}