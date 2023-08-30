using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Shared
{
    public interface IPageable<T> : IEnumerable<T>
    {
        int PageNumber { get; }
        int PageSize { get; }
        long TotalItems { get; }
        int TotalPages { get; }
    }

    public class Pageable<T> : IPageable<T>
    {
        public Pageable(int pageNumber, int pageSize, long totalItems, IEnumerable<T> items)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
            Items = items;
        }

        public int PageNumber { get; }
        public int PageSize { get; }
        public long TotalItems { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        private IEnumerable<T> Items { get; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPrevPage => PageNumber > 1;

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
