using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class MediaRepository : AppRepository<Media>, IMediaRepository
    {
        public MediaRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
