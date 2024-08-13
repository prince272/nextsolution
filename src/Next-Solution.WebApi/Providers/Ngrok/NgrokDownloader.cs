// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using CliWrap;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Next_Solution.WebApi.Providers.Ngrok
{
    public class NgrokDownloader : INgrokDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NgrokDownloader> _logger;

        public NgrokDownloader(
            HttpClient httpClient,
            ILogger<NgrokDownloader> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task DownloadExecutableAsync(CancellationToken cancellationToken)
        {
            var downloadUrl = GetDownloadPath();

            var zipFileName = $"{GetOsArchitectureString()}{GetCompressionFileFormat()}";
            if (!File.Exists(GetCompressedDownloadFileName()))
            {
                _logger.LogTrace("Downloading {DownloadUrl}", downloadUrl);
                await DownloadFileAsync(downloadUrl, cancellationToken);
                _logger.LogTrace("Downloaded {DownloadUrl}", downloadUrl);
            }
            else
            {
                _logger.LogTrace("Ngrok compressed file has already been downloaded.");
            }

            var ngrokFileName = GetExecutableFileName();
            if (!File.Exists(ngrokFileName))
            {
                _logger.LogTrace("Extracting {ZipFileName} to {NgrokFileName}", zipFileName, ngrokFileName);
                await ExtractCompressedFileToCurrentDirectoryAsync();
                _logger.LogTrace("Extracted {ZipFileName} to {NgrokFileName}", zipFileName, ngrokFileName);
            }
            else
            {
                _logger.LogTrace("Ngrok executable already has been extracted.");
            }
        }

        private static async Task ExtractCompressedFileToCurrentDirectoryAsync()
        {
            if (GetCompressionFileFormat() == ".zip")
            {
                ZipFile.ExtractToDirectory(
                    GetCompressedDownloadFileName(),
                    Directory.GetCurrentDirectory(),
                    true);
            }
            else if (GetCompressionFileFormat() == ".tgz")
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    await Cli.Wrap("unzip")
                        .WithArguments(args => args
                            .Add(GetCompressedDownloadFileName()))
                        .WithWorkingDirectory(Environment.CurrentDirectory)
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();
                }
                else
                {
                    await Cli.Wrap("tar")
                        .WithArguments(args => args
                            .Add("xvzf")
                            .Add(GetCompressedDownloadFileName()))
                        .WithWorkingDirectory(Environment.CurrentDirectory)
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();
                }
            }
        }

        private async Task DownloadFileAsync(string downloadUrl, CancellationToken cancellationToken)
        {
            var downloadResponse = await _httpClient.GetAsync(downloadUrl, cancellationToken);
            downloadResponse.EnsureSuccessStatusCode();

            var downloadStream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
            await using var writer = File.Create(GetCompressedDownloadFileName());
            await downloadStream.CopyToAsync(writer, cancellationToken);
        }

        public static string GetExecutableFileName()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Ngrok.exe" : "ngrok";
        }

        private static string GetCompressionFileFormat()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ".tgz" : ".zip";
        }

        private static string GetDownloadPath()
        {
            const string cdnPath = "c/bNyj1mQVY4c";
            var fileName = GetCompressedDownloadFileName();
            return $"{cdnPath}/{fileName}";
        }

        private static string GetCompressedDownloadFileName()
        {
            var fileName = $"ngrok-v3-stable-{GetOsArchitectureString()}{GetCompressionFileFormat()}";
            return fileName;
        }

        private static string GetArchitectureString()
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            switch (architecture)
            {
                case Architecture.Arm:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        throw new NgrokUnsupportedException();
                    }

                    return "arm";
                case Architecture.Arm64:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        throw new NgrokUnsupportedException();
                    }

                    return "arm64";

                case Architecture.X64:
                    return "amd64";

                case Architecture.X86:
                    return "386";

                case Architecture.Wasm:
                case Architecture.S390x:
                default:
                    throw new NgrokUnsupportedException();
            }
        }

        private static string GetOsString()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "windows";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "darwin";
            }

            throw new NgrokUnsupportedException();
        }

        private static string GetOsArchitectureString()
        {
            return $"{GetOsString()}-{GetArchitectureString()}";
        }
    }
}