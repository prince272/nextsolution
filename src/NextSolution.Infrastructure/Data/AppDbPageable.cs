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
    public class AppDbPageable<T> : IPageable<T>
    {
        public AppDbPageable(int pageNumber, int pageSize, long totalItems, IEnumerable<T> items)
        {
            Page = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
            Items = items;
        }

        public int Page { get; }
        public int PageSize { get; }
        public long TotalItems { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public IEnumerable<T> Items { get; }
        public bool HasNextPage => Page < TotalPages;
        public bool HasPrevPage => Page > 1;
    }

    public static class PageableExtensions
    {
        public static async Task<AppDbPageable<T>> PaginateAsync<T>(
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

            return new AppDbPageable<T>(pageNumber, pageSize, totalItems, items);
        }
    }
}
