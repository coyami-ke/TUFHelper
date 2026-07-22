using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;

namespace DirectLevel
{
    public class LevelDownloader
    {
        private LevelListInfoElementJson _levelInfo;

        public Action<Exception> ErrorHandler;

        public delegate void UpdateProgressEventHandler(object sender, UpdateProgressEventArgs args);
        public event UpdateProgressEventHandler UpdateProgress;

        public delegate void DownloadCompleteEventHandler(object sender, DownloadCompleteEventArgs args);
        public event DownloadCompleteEventHandler DownloadComplete;

        public Func<long, bool> OnCalculationCompleteFileSize;

        public LevelDownloader(LevelListInfoElementJson levelInfo)
        {
            _levelInfo = levelInfo;
        }

        public static List<string> FindAdofaiFiles(string path)
        {
            var result = new List<string>();

            if (!Directory.Exists(path)) return result;

            try
            {
                var files = new DirectoryInfo(path).GetFiles("*.adofai", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (file.Name.Contains("backup")) continue;

                    result.Add(file.FullName);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Main.Logger.Error($"Access denied to some folders in path {path}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Failed searching directories: {ex.Message}");
            }

            return result.OrderByDescending(f => new FileInfo(f).Length).ToList();
        }

        private static readonly char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();
        public static string MakeSafeFilename(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unknown";

            string clean = string.Join("_", name.Split(InvalidFilenameChars, StringSplitOptions.RemoveEmptyEntries));

            clean = clean.Trim().TrimEnd('.');

            return string.IsNullOrEmpty(clean) ? "Restricted_Name" : clean;
        }

        public static string GetPathToLevelFolder(string defaultPath, string song, string artist, int id)
        {
            string safeArtist = MakeSafeFilename(artist);
            string safeSong = MakeSafeFilename(song);

            return Path.Combine(defaultPath, $"{safeArtist} - {safeSong} {id}");
        }

        public Task DownloadWithTask(string defaultPath, bool checkFileSize, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Preparing));

                    var path = GetPathToLevelFolder(defaultPath, _levelInfo.Song, _levelInfo.Artist, _levelInfo.ID);
                    var zipPath = $"{path}.zip";

                    if (!File.Exists(zipPath) && Directory.Exists(path) && Directory.GetFiles(path).Length > 0)
                    {
                        UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Downloaded));
                        DownloadComplete?.Invoke(this, new DownloadCompleteEventArgs(FindAdofaiFiles(path)));
                        Main.DownloadedLevels.SaveLevel(_levelInfo);
                        return;
                    }

                    token.ThrowIfCancellationRequested();

                    if (checkFileSize && OnCalculationCompleteFileSize != null)
                    {
                        long fileSize = await GetRemoteFileSizeAsync(_levelInfo.DlLink, token);
                        if (fileSize > 0)
                        {
                            bool cancelDownload = OnCalculationCompleteFileSize.Invoke(fileSize);
                            if (cancelDownload)
                            {
                                UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Cancelled));
                                return;
                            }
                        }
                    }

                    if (File.Exists(zipPath)) File.Delete(zipPath);

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Downloading));

                    await DownloadFileWithProgressAsync(_levelInfo.DlLink, zipPath, token);

                    token.ThrowIfCancellationRequested();

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Unzipping));

                    ZipExtractor.Unzip(zipPath, path);
                    File.Delete(zipPath);

                    Utils.MoveLastDirectory(path, path);
                    GC.Collect();

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Downloaded));
                    DownloadComplete?.Invoke(this, new DownloadCompleteEventArgs(FindAdofaiFiles(path)));

                    Main.DownloadedLevels.SaveLevel(_levelInfo);
                }
                catch (OperationCanceledException)
                {
                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Cancelled));
                }
                catch (Exception ex)
                {
                    ErrorHandler?.Invoke(ex);
                }
            }, token);
        }
        private async Task<long> GetRemoteFileSizeAsync(string url, CancellationToken token)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                using var response = await Main.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength.HasValue)
                {
                    return response.Content.Headers.ContentLength.Value;
                }

            }
            catch
            {
                // Fallback gracefully if the CDN server strictly denies HEAD method requests
            }
            return -1;
        }
        private async Task DownloadFileWithProgressAsync(string url, string destinationPath, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            using var response = await Main.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalBytesReceived = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) != 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                totalBytesReceived += bytesRead;

                UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(totalBytesReceived, totalBytes ?? totalBytesReceived));
            }
        }
    }
}