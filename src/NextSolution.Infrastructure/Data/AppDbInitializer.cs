using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextSolution.Core.Constants;
using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Data
{
    // How to seed data in .NET Core 6 with Entity Framework?
    // source: https://stackoverflow.com/questions/70581816/how-to-seed-data-in-net-core-6-with-entity-framework
    public class AppDbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            // Get a logger
            var logger = services.GetRequiredService<ILogger<AppDbInitializer>>();


            var dbContext = services.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

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
    }
}