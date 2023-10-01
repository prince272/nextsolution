namespace NextSolution.Core.Utilities
{
    public static class UriExtensions
    {
        public static Uri CombinePaths(this Uri uri, params string[] paths)
        {
            return new Uri(paths.SelectMany(path => path.Split('/', StringSplitOptions.RemoveEmptyEntries)).Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
        }
    }
}
