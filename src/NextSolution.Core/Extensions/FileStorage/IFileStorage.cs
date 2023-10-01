namespace NextSolution.Core.Extensions.FileStorage
{
    public interface IFileStorage
    {
        Task WriteAsync(string path, Stream content, CancellationToken cancellationToken = default);

        Task<FileChunkStatus> WriteAsync(string path, Stream chunk, long length, long offset, CancellationToken cancellationToken = default);

        Task<Stream?> ReadAsync(string path, CancellationToken cancellationToken = default);

        Task DeleteAsync(string path, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

        Task<string> GetPublicUrlAsync(string path, CancellationToken cancellationToken = default);
    }

    public enum FileChunkStatus
    {
        Started,
        Processing,
        Completed,
    }
}
