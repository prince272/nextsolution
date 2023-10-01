using System.Collections;

namespace NextSolution.Core.Utilities
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> LongSkip<T>(this IQueryable<T> items, long count)
            => LongSkip(items, int.MaxValue, count);

        internal static IQueryable<T> LongSkip<T>(this IQueryable<T> items, int segmentSize, long count)
        {
            long segmentCount = Math.DivRem(count, segmentSize, out long remainder);

            for (long i = 0; i < segmentCount; i += 1)
                items = items.Skip(segmentSize);

            if (remainder != 0)
                items = items.Skip((int)remainder);

            return items;
        }
    }

    public interface IPageable<T> : IEnumerable<T>
    {
        long Offset { get; }
        int Limit { get; }
        long Length { get; }
        long? Previous { get; }
        long? Next { get; }
    }

    public class Pageable<T> : IPageable<T>
    {
        public Pageable(long offset, int limit, long total, IEnumerable<T> items)
        {
            Offset = offset;
            Limit = limit;
            Length = total;
            Items = items;
        }

        public long Offset { get; }
        public int Limit { get; }
        public long Length { get; }
        public IEnumerable<T> Items { get; }
        public long? Previous => Offset - Limit >= 0 ? Offset - Limit : null;
        public long? Next => Offset + Limit < Length ? Offset + Limit : null;

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