using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.FileStorage
{
    public interface IFileStorage
    {
        Task WriteAsync(string directoryName, string fileName, Stream content);

        Task<long> WriteAsync(string directoryName, string fileName, Stream chunk, long length, long offset);

        Task<Stream?> ReadAsync(string directoryName, string fileName);

        Task DeleteAsync(string directoryName, string fileName);

        Task<bool> ExistsAsync(string directoryName, string fileName);
    }
}
