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
}
