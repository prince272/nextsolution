using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Utilities;

namespace NextSolution.Infrastructure.FileStorage.Local
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IOptions<LocalFileStorageOptions> _storageOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalFileStorage(IOptions<LocalFileStorageOptions> storageOptions, IHttpContextAccessor httpContextAccessor)
        {
            _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task WriteAsync(string path, Stream content, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var filePath = GetFilePath(path);
            using var fileStream = File.Create(filePath);
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        public async Task<FileChunkStatus> WriteAsync(string path, Stream chunk, long length, long offset, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));

            var tempFilePath = GetTempFilePath(path);

            try
            {
                var chunkStatus = !File.Exists(tempFilePath) ? FileChunkStatus.Started : FileChunkStatus.Processing;

                using (var tempFileStream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    tempFileStream.Seek(offset, SeekOrigin.Begin);
                    await chunk.CopyToAsync(tempFileStream, cancellationToken);
                }

                var fileLength = new FileInfo(tempFilePath).Length;

                if (fileLength >= length)
                {
                    var filePath = GetFilePath(path);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.Move(tempFilePath, filePath);

                    chunkStatus = FileChunkStatus.Completed;
                }

                return chunkStatus;
            }
            finally
            {
                // Ensure that the stream and resources are properly disposed
                chunk.Dispose();
            }
        }

        public Task<Stream?> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filePath = GetFilePath(path);

            if (File.Exists(filePath))
            {
                return Task.FromResult((Stream?)new FileStream(filePath, FileMode.Open, FileAccess.Read));
            }

            return Task.FromResult<Stream?>(null);
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filePath = GetFilePath(path);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filePath = GetFilePath(path);
            return Task.FromResult(File.Exists(filePath));
        }

        public Task<string> GetPublicUrlAsync(string path, CancellationToken cancellationToken = default)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return Task.FromResult(GetFileUrl(path));
        }

        private string GetFileUrl(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var request = (_httpContextAccessor.HttpContext?.Request) ?? throw new InvalidOperationException();
            var baseUrl = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}");
            var url = baseUrl.CombinePaths(_storageOptions.Value.WebRootPath, path);
            return url.ToString();
        }

        private string GetFilePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var fileName = Path.GetFileName(path);

            var invalidFileNameChars = fileName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
            if (invalidFileNameChars.Length > 0) throw new ArgumentException($"Invalid characters in file name: {string.Join(", ", invalidFileNameChars)}");

            var directoryNames = Path.GetDirectoryName(path)?.Split(new char[] { '/', '\\' }) ?? Array.Empty<string>();

            foreach (var directoryName in directoryNames)
            {
                var invalidDirectoryNameChars = directoryName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
                if (invalidDirectoryNameChars.Length > 0) throw new ArgumentException($"Invalid characters in directory name: {string.Join(", ", invalidDirectoryNameChars)}");
            }

            var filePath = Path.Combine(_storageOptions.Value.RootPath, path.Replace('/', '\\').TrimStart('\\', '/'));

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            }

            return filePath;
        }

        private string GetTempFilePath(string path)
        {
            return GetFilePath(path) + ".temp";
        }
    }
}