using System.Text;

namespace NextSolution.Core.Utilities
{

    public static class StreamExtensions
    {
        public static async Task<byte[]> ToByteArrayAsync(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public static async Task<string> ReadAsStringAsync(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize = 81920)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead);
            }
        }

        public static async Task<int> WriteStringAsync(this Stream stream, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            await stream.WriteAsync(bytes);
            return bytes.Length;
        }

        public static async Task<Stream> ToMemoryStreamAsync(this Stream stream)
        {
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}