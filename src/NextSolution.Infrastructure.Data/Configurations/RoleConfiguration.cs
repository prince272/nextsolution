using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextSolution.Core.Entities;

namespace NextSolution.Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role");

            // Each Role can have many entries in the UserRole join table
            builder.HasMany(r => r.Users)
                .WithOne(r => r.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        }
    }

    public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<long>>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleClaim<long>> builder) => builder.ToTable("RoleClaim");
    }
}
