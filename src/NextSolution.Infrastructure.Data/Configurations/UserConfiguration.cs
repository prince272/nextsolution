using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextSolution.Core.Entities;

namespace NextSolution.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");

            // Each User can have many entries in the UserRole join table
            builder.HasMany(u => u.Roles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        }
    }

    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder) => builder.ToTable("UserRole");
    }

    public class UserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<long>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<long>> builder) => builder.ToTable("UserClaim");
    }

    public class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<long>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<long>> builder) => builder.ToTable("UserLogin");
    }

    public class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<long>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<long>> builder) => builder.ToTable("UserToken");
    }

    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder) => builder.ToTable(nameof(UserSession));
    }
}
