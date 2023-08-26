using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Shared
{
    public interface IPageable<T>
    {
        int Page { get; }
        int PageSize { get; }
        long TotalItems { get; }
        int TotalPages { get; }
        IEnumerable<T> Items { get; }
    }

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
}
