using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TUFHelper;
using TUFHelper.ModScripts.Web;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DirectLevel
{
    public class LevelDownloader
    {
        /// <summary>
        /// WebClient for Google Drive download with built-in timeouts and cookie support
        /// </summary>
        private class CookieWebClient : WebClient
        {
            private class ManualCookieContainer
            {
                private readonly Dictionary<string, string> cookies = new Dictionary<string, string>();

                public string this[Uri address]
                {
                    get
                    {
                        return cookies.TryGetValue(address.Host, out var cookie) ? cookie : null;
                    }
                    set
                    {
                        cookies[address.Host] = value;
                    }
                }
            }

            private readonly ManualCookieContainer _cookies = new ManualCookieContainer();

            protected override WebRequest GetWebRequest(Uri u)
            {
                var r = base.GetWebRequest(u);
                if (r is HttpWebRequest request)
                {
                    // Add a realistic user agent to prevent servers like MediaFire/Google from throttling or blocking
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                    // Explicitly set network timeout parameters (30-second connection handshake)
                    request.Timeout = 30000;
                    // 5-minute read timeout window for downloading massive level zips
                    request.ReadWriteTimeout = 300000;

                    var c = _cookies[u];
                    if (c != null)
                    {
                        request.Headers.Set("cookie", c);
                    }
                }
                return r;
            }
        }

        private string _url;
        private CookieWebClient _cookieWeb;

        public Action<Exception> ErrorHandler;

        public delegate void UpdateProgressEventHandler(object sender, UpdateProgressEventArgs args);
        public event UpdateProgressEventHandler UpdateProgress;

        public delegate void DownloadCompleteEventHandler(object sender, DownloadCompleteEventArgs args);
        public event DownloadCompleteEventHandler DownloadComplete;

        public Func<long, bool> OnCalculationCompleteFileSize;

        public LevelDownloader(string url)
        {
            _cookieWeb = new CookieWebClient();
            _cookieWeb.Encoding = Encoding.UTF8;
            _cookieWeb.Proxy = null;
            _url = url;
        }

        public static List<string> FindAdofaiFiles(string path)
        {
            var result = new List<string>();

            foreach (var file in new DirectoryInfo(path).GetFiles())
            {
                if (!file.Extension.ToLower().Contains("adofai")) continue;
                if (file.Name.Contains("backup")) continue;

                result.Add(file.FullName);
            }

            return result.OrderByDescending(f => new FileInfo(f).Length).ToList();
        }

        public Task DownloadWithTask(string defaultPath, bool checkFileSize, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Preparing));

                    var path = Path.Combine(defaultPath, _url.GetHashCode().ToString());
                    var zipPath = $"{path}.zip";

                    if (!File.Exists(zipPath) && Directory.Exists(path) && Directory.GetFiles(path).Length > 0)
                    {
                        UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Downloaded));
                        DownloadComplete?.Invoke(this, new DownloadCompleteEventArgs(FindAdofaiFiles(path)));
                        return;
                    }

                    // Hand off the WebClient to resolve redirection chains securely
                    var directURL = GetDirectURL(_url, _cookieWeb);
                    token.ThrowIfCancellationRequested();

                    // File size check execution
                    if (checkFileSize && OnCalculationCompleteFileSize != null)
                    {
                        var v = OnCalculationCompleteFileSize.Invoke(Utils.GetFileSize(directURL));
                        if (v)
                        {
                            UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Cancelled));
                            return;
                        }
                    }

                    if (File.Exists(zipPath)) File.Delete(zipPath);

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    _cookieWeb.DownloadProgressChanged += (sender, args) =>
                    {
                        UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(args.BytesReceived, args.TotalBytesToReceive));
                    };

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Downloading));

                    // Link the cancellation token safely to the WebClient request handler
                    using (token.Register(() => _cookieWeb.CancelAsync()))
                    {
                        await _cookieWeb.DownloadFileTaskAsync(directURL, zipPath);
                    }

                    token.ThrowIfCancellationRequested();

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Unzipping));

                    ZipHelper.Unzip(zipPath, path);
                    File.Delete(zipPath);

                    Utils.MoveLastDirectory(path, path);
                    GC.Collect();

                    UpdateProgress?.Invoke(this, new UpdateProgressEventArgs(LevelDownloaderStates.Downloaded));
                    DownloadComplete?.Invoke(this, new DownloadCompleteEventArgs(FindAdofaiFiles(path)));
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

        private static string GetDirectURL(string url, WebClient wc)
        {
            try
            {
                // Fix Discord CDN Links
                if (url.Contains("cdn.discordapp.com"))
                    return url.Replace("cdn.discordapp.com", "fixcdn.hyonsu.com");

                // Google Drive Address Extraction Framework
                if (url.StartsWith("https://drive.google.com/file/d/") ||
                    url.StartsWith("https://drive.google.com/open?id=") ||
                    url.StartsWith("https://drive.google.com/u/0/uc"))
                {
                    if (url.StartsWith("https://drive.google.com/u/0/uc"))
                        return GetDirectURLFromGoogleLargeFile(url, wc);

                    var driveid = "";
                    if (url.Contains("/d/"))
                    {
                        if (url.Contains("/view")) driveid = url.GetValue("/d/", "/view");
                        else if (url.Contains("/edit")) driveid = url.GetValue("/d/", "/edit");
                    }

                    if (url.Contains("id="))
                    {
                        driveid = url.StringSplit("id=")[1];
                        if (driveid.Contains("&"))
                            driveid = driveid.StringSplit("&")[0];
                    }

                    if (string.IsNullOrEmpty(driveid))
                        throw new Exception($"Google Drive ID not resolved.\nURL: {url}");

                    var downloadURL = $"https://drive.google.com/u/0/uc?export=download&id={driveid}";

                    // Synchronous blocking network check—now bounded safely by our custom web timeouts
                    using (var stream = wc.OpenRead(downloadURL))
                    {
                        var buffer = new byte[15];
                        stream.Read(buffer, 0, buffer.Length);

                        if (Encoding.UTF8.GetString(buffer) == "<!DOCTYPE html>")
                        {
                            return GetDirectURLFromGoogleLargeFile(downloadURL, wc);
                        }
                    }

                    return downloadURL;
                }

                // MediaFire Parsing
                if (url.StartsWith("https://www.mediafire.com"))
                {
                    var mfhtml = wc.DownloadString(url);
                    var indexurl = mfhtml.IndexOf("https://download");
                    var indexend = Utils.GetNextIndexOf('"', mfhtml, indexurl);

                    return mfhtml.Substring(indexurl, indexend - indexurl);
                }

                // Dropbox Formatting
                if (url.StartsWith("https://www.dropbox.com"))
                {
                    var driveid = url.GetValue("https://www.dropbox.com/s/", "?");
                    return $"https://www.dropbox.com/s/{driveid}?dl=1";
                }

                if (url.StartsWith("https://drive.google.com/drive/folders/"))
                    throw new Exception($"Google Drive folders are not supported directly.\nURL: {url}");

                if (url.StartsWith("https://steamcommunity.com/"))
                    throw new Exception($"Steam Workshop links are not supported directly.\nURL: {url}");

                return url;
            }
            catch (Exception e)
            {
                throw new Exception($"The download link is not accessible.\nException Details: {e.Message}");
            }
        }

        private static string GetDirectURLFromGoogleLargeFile(string url, WebClient wc)
        {
            var result = wc.DownloadString(url);
            var id = result.GetValue("name=\"id\" value=\"", "\">");
            var uuid = result.GetValue("name=\"uuid\" value=\"", "\">");

            if (!result.Contains("name=\"at\" value=\""))
                return $"https://drive.usercontent.google.com/download?id={id}&export=download&authuser=0&confirm=t&uuid={uuid}";

            var at = result.GetValue("name=\"at\" value=\"", "\">");
            return $"https://drive.usercontent.google.com/download?id={id}&export=download&authuser=0&confirm=t&uuid={uuid}&at={at}";
        }
    }
}