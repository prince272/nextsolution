using Microsoft.Extensions.Options;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Utilities;
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

        public async Task WriteAsync(string directoryName, string fileName, Stream content)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var filePath = GetFilePath(directoryName, fileName);
            using var fileStream = File.Create(filePath);
            await content.CopyToAsync(fileStream);
        }

        public async Task<long> WriteAsync(string directoryName, string fileName, Stream chunk, long length, long offset)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));

            var tempFilePath = GetTempFilePath(directoryName, fileName);

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
                    var filePath = GetFilePath(directoryName, fileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.Move(tempFilePath, filePath);
                }

                return fileLength;
            }
            finally
            {
                // Ensure that the stream and resources are properly disposed
                chunk.Dispose();
            }
        }

        public Task<Stream?> ReadAsync(string directoryName, string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var filePath = GetFilePath(directoryName, fileName);

            if (File.Exists(filePath))
            {
                return Task.FromResult((Stream?)new FileStream(filePath, FileMode.Open, FileAccess.Read));
            }

            return Task.FromResult<Stream?>(null);
        }

        public Task DeleteAsync(string directoryName, string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var filePath = GetFilePath(directoryName, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string directoryName, string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var filePath = GetFilePath(directoryName, fileName);
            return Task.FromResult(File.Exists(filePath));
        }

        private string GetFilePath(string directoryName, string fileName)
        {
            if (directoryName == null) throw new ArgumentNullException(nameof(directoryName));
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var invalidDirectoryNameChars = directoryName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
            if (invalidDirectoryNameChars.Length > 0) throw new ArgumentException($"Invalid characters in directory name: {string.Join(", ", invalidDirectoryNameChars)}");

            var invalidFileNameChars = fileName.Where(c => Path.GetInvalidPathChars().Concat(new[] { '/', '\\' }).Contains(c)).ToArray();
            if (invalidFileNameChars.Length > 0) throw new ArgumentException($"Invalid characters in file name: {string.Join(", ", invalidFileNameChars)}");

            var directoryPath = directoryName = Path.Combine(_storageOptions.Value.RootPath, directoryName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var fileExtension = Path.GetExtension(fileName)?.ToLower();

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            var filePath = Path.Combine(directoryName, $"{fileNameWithoutExtension}{fileExtension}");
            return filePath;
        }

        private string GetTempFilePath(string directoryName, string fileName)
        {
            return GetFilePath(directoryName, fileName) + ".temp";
        }
    }
}