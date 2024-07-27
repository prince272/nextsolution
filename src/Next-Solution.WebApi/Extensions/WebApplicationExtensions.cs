using Microsoft.EntityFrameworkCore;

namespace Next_Solution.WebApi.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task RunDbMigrationsAsync<TContext>(this WebApplication app) where TContext : DbContext
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                var dbContext = services.GetRequiredService<TContext>();

                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var count = pendingMigrations.Count();

                if (count > 0)
                {
                    logger.LogInformation("Found {Count} pending migration(s) to apply. Proceeding with migration...", count);
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Successfully applied {Count} pending migration(s).", count);
                }
                else
                {
                    logger.LogInformation("No pending migrations found.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while performing database migration.");
            }
        }
    }
}
