using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextSolution.Core.Extensions.FileStorage
{
    public interface IFileStorage
    {
        Task WriteAsync(string fileName, Stream content);

        Task WriteAsync(string fileName, Stream chunk, long length, long offset);

        Task<Stream?> ReadAsync(string fileName);

        Task DeleteAsync(string fileName);

        Task<bool> ExistsAsync(string fileName);
    }
}
