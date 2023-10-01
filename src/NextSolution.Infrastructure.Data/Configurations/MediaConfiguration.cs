using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NextSolution.Core.Entities;

namespace NextSolution.Infrastructure.Data.Configurations
{
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.ToTable(nameof(Media));

            builder.HasIndex(m => new { m.Path }).IsUnique();
        }
    }
}