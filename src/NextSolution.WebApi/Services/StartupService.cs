using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using NextSolution.Infrastructure.Data;

namespace NextSolution.WebApi.Services
{
    public class StartupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
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
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }

            try
            {
                logger.LogInformation("Start seeding the database.");

                var roleRepository = services.GetRequiredService<IRoleRepository>();

                if (!(await roleRepository.AnyAsync(cancellationToken)))
                {
                    foreach (var roleName in Role.All)
                    {
                        await roleRepository.CreateAsync(new Role(roleName), cancellationToken);
                    }
                }


                //var chatRepository = services.GetRequiredService<IChatRepository>();

                //if (!(await chatRepository.AnyAsync(cancellationToken)))
                //{
                //    var user = (await services.GetRequiredService<IUserRepository>().GetAsync(_ => true))!;

                //    foreach (var _ in Enumerable.Range(1, 100))
                //    {
                //        var recentDateTime = new Bogus.Faker().Date.RecentOffset(10);
                //        var chat = new Chat
                //        {
                //            Title = new Bogus.Faker().Lorem.Sentence(2),
                //            CreatedAt = recentDateTime,
                //            UpdatedAt = recentDateTime,
                //            UserId = user.Id
                //        };
                //        await chatRepository.CreateAsync(chat);
                //    }
                //}

                logger.LogInformation("Finished seeding the database.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }

            try
            {
                var clientRepository = services.GetRequiredService<IClientRepository>();
                await clientRepository.DeactivateAllAsync(cancellationToken);
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