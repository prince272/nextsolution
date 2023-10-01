using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NextSolution.Core.Entities;
using NextSolution.Core.Utilities;
using NextSolution.Infrastructure.Data.Extensions;

namespace NextSolution.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, long, IdentityUserClaim<long>,
        UserRole, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>
    {
        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var assemblies = AssemblyHelper.GetAssemblies();

            builder.ApplyEntities(assemblies);
            builder.ApplyConfigurations(assemblies);
        }
    }
}