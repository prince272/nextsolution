namespace NextSolution.Infrastructure.FileStorage.Local
{
    public class LocalFileStorageOptions
    {
        public string RootPath { get; set; } = default!;

        public string WebRootPath { get; set; } = default!;
    }
}
