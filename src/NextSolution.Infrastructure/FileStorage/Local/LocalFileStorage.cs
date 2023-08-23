using Microsoft.Extensions.Options;
using NextSolution.Core.Extensions.FileStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Infrastructure.FileStorage.Local
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IOptions<LocalFileStorageOptions> _storageOptions;

        public LocalFileStorage(IOptions<LocalFileStorageOptions> storageOptions)
        {
            _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
        }

        public async Task WriteAsync(string fileName, Stream content)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var filePath = GetFilePath(fileName);
            using var fileStream = File.Create(filePath);
            await content.CopyToAsync(fileStream);
        }

        public async Task WriteAsync(string fileName, Stream chunk, long length, long offset)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));

            var tempFilePath = GetTempFilePath(fileName);

            try
            {
                using (var tempFileStream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    tempFileStream.Seek(offset, SeekOrigin.Begin);
                    await chunk.CopyToAsync(tempFileStream);
                }

                var fileLength = new FileInfo(tempFilePath).Length;

                if (fileLength >= length)
                {
                    var filePath = GetFilePath(fileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.Move(tempFilePath, filePath);
                }
            }
            finally
            {
                // Ensure that the stream and resources are properly disposed
                chunk.Dispose();
            }
        }

        public Task<Stream?> ReadAsync(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var filePath = GetFilePath(fileName);

            if (File.Exists(filePath))
            {
                return Task.FromResult((Stream?)new FileStream(filePath, FileMode.Open, FileAccess.Read));
            }

            return Task.FromResult<Stream?>(null);
        }

        public Task DeleteAsync(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var filePath = GetFilePath(fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var filePath = GetFilePath(fileName);
            return Task.FromResult(File.Exists(filePath));
        }

        private string GetFilePath(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var invalidChars = Path.GetInvalidFileNameChars();
            var invalidChar = fileName.FirstOrDefault(c => invalidChars.Contains(c));
            if (invalidChar != 0) throw new ArgumentException($"Invalid character '{invalidChar}' found in '{fileName}'.", nameof(fileName));

            var directoryPath = _storageOptions.Value.RootPath;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var fileExtension = Path.GetExtension(fileName)?.ToLower();

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}{fileExtension}");
            return filePath;
        }

        private string GetTempFilePath(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var invalidChars = Path.GetInvalidFileNameChars();
            var invalidChar = fileName.FirstOrDefault(c => invalidChars.Contains(c));
            if (invalidChar != 0) throw new ArgumentException($"Invalid character '{invalidChar}' found in '{fileName}'.", nameof(fileName));

            var directoryPath = _storageOptions.Value.RootPath;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var fileExtension = Path.GetExtension(fileName)?.ToLower();

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var tempFilePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}{fileExtension}.temp");
            return tempFilePath;
        }
    }
}
