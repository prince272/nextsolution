using Microsoft.AspNetCore.Identity;
using NextSolution.Core.Entities;
using NextSolution.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.Data.Repositories
{
    public class MediaRepository : AppRepository<Media>, IMediaRepository
    {
        public MediaRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
