using Humanizer;
using Microsoft.EntityFrameworkCore;
using NextSolution.Core.Shared;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NextSolution.Infrastructure.Data
{
    public static class PageableExtensions
    {
        public static async Task<Pageable<T>> PaginateAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1.");

            long totalItems = await source.LongCountAsync(cancellationToken);
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new Pageable<T>(pageNumber, pageSize, totalItems, items);
        }
    }
}
