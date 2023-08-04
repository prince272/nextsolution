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
    public class Pageable<T> : IPageable<T>
    {
        public Pageable(int pageNumber, int pageSize, long totalItems, IEnumerable<T> items)
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
        public static Pageable<T> Paginate<T>(
            this IEnumerable<T> source,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1.");

            long totalItems = source.LongCount();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new Pageable<T>(pageNumber, pageSize, totalItems, items);
        }

        public static async Task<Pageable<T>> PaginateAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1.");

            long totalItems = await source.LongCountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new Pageable<T>(pageNumber, pageSize, totalItems, items);
        }

        public static Pageable<TResult> Paginate<TSource, TResult>(
            this IEnumerable<TSource> source,
            int pageNumber,
            int pageSize,
            Func<TSource, TResult> selector)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1.");

            long totalItems = source.LongCount();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(selector).ToList();

            return new Pageable<TResult>(pageNumber, pageSize, totalItems, items);
        }

        public static async Task<Pageable<TResult>> PaginateAsync<TSource, TResult>(
            this IQueryable<TSource> source,
            int pageNumber,
            int pageSize,
            Func<TSource, TResult> selector)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1.");

            long totalItems = await source.LongCountAsync();
            var items = (await (source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync())).Select(selector).ToList();

            return new Pageable<TResult>(pageNumber, pageSize, totalItems, items);
        }
    }
}
