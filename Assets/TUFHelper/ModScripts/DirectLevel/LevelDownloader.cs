using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DirectLevel
{
    public class LevelDownloader
    {
        /// <summary>
        /// WebClient for Google Drive download
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
                        string cookie;
                        return cookies.TryGetValue(address.Host, out cookie) ? cookie : null;
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
                if(r is HttpWebRequest request)
                {
                    var c = _cookies[u];
                    if( _cookies != null )
                        request.Headers.Set("cookie", c);
                }
                return r;
            }
        }
        
        public enum ForumType
        {
            ADOFAI_GG,
            TUC
        }
        
        private string _url;
        private CookieWebClient _cookieWeb;
        
        /// <summary>
        /// Run when an exception occurs
        /// Runs in a thread other than the main thread!!!
        /// </summary>
        public Action<Exception> ErrorHandler;
        
        /// <summary>
        /// Run if progress changes when downloading levels asynchronously
        /// Runs in a thread other than the main thread!!!
        /// </summary>
        public Action<float, string> OnUpdateProgress;
        
        
        /// <summary>
        /// Run when asynchronously leveling down is complete.
        /// Runs in a thread other than the main thread!!!
        /// </summary>
        public Action<List<string>> OnDownloadComplete;
        
        /// <summary>
        /// Run when getting the size of a file from a URL.
        /// The return type determines whether the download should continue or not.
        /// Runs in a thread other than the main thread!!!
        /// </summary>
        public Func<long, bool> OnCalculationCompleteFileSize;

        
        /// <summary>
        /// Level Downloader
        /// </summary>
        /// <param name="url">URL to download, may not be a direct URL if in Google Drive</param>
        public LevelDownloader(string url)
        {
            
            _cookieWeb = new CookieWebClient();
            _cookieWeb.Encoding = Encoding.UTF8;
            _cookieWeb.Proxy = null;

            _url = url;

        }
        

        /// <summary>
        /// Find the .adofai files in Level folder
        /// </summary>
        /// <param name="path">Level Folder</param>
        /// <returns>.adofai files</returns>
        private static List<string> FindAdofaiFiles(string path)
        {
            var result = new List<string>();
            
            foreach (var file in new DirectoryInfo(path).GetFiles())
            {
                
                if (!file.Extension.ToLower().Contains("adofai")) continue;
                if (file.Name.Contains("backup")) continue;
                
                result.Add(file.FullName);
                
            }
            
            return result;
        }
        
        
        
        
        /// <summary>
        /// Asynchronous level downloads
        /// </summary>
        /// <param name="defaultPath">Default parent path to download</param>
        /// <param name="checkFileSize">If true, check the file size</param>
        /// <returns>Tasks that are downloading</returns>
        public Task DownloadWithTask(string defaultPath, bool checkFileSize)
        {
            return Task.Run(() =>
            {
                try
                {
                    OnUpdateProgress?.Invoke(0, "Preparing");

                    GC.Collect();

                    var path = Path.Combine(defaultPath, _url.GetHashCode().ToString());
                    var zipPath = $"{path}.zip";

                    if (!File.Exists(zipPath) && Directory.Exists(path) && Directory.GetFiles(path).Length > 0)
                    {
                        OnUpdateProgress?.Invoke(1, "Downloaded");
                        OnDownloadComplete?.Invoke(FindAdofaiFiles(path));
                        return;
                    }
                    
                    var directURl = GetDirectURL(_url, _cookieWeb);
                    
                    if (checkFileSize)
                    {
                        if (OnCalculationCompleteFileSize != null)
                        {
                            var v = OnCalculationCompleteFileSize.Invoke(Utils.GetFileSize(directURl));
                            if (v)
                            {
                                OnUpdateProgress?.Invoke(0, "Cancelled");
                                return;
                            }
                        }
                    }



                    if (File.Exists(zipPath)) File.Delete(zipPath);

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    _cookieWeb.DownloadProgressChanged += (sender, args) =>
                    {
                        OnUpdateProgress?.Invoke((1 + (args.BytesReceived / (float)args.TotalBytesToReceive)) / 3,
                            $"Downloading ({Utils.ByteToStringUnit(args.BytesReceived)}/{Utils.ByteToStringUnit(args.TotalBytesToReceive)})");
                    };

                    OnUpdateProgress?.Invoke(0.3333f, "Downloading");
                    var t = _cookieWeb.DownloadFileTaskAsync(directURl, zipPath);
                    t.Wait();

                    OnUpdateProgress?.Invoke(0.6666f, "Unzipping");
                    ZipHelper.Unzip(zipPath, path);
                    File.Delete(zipPath);

                    Utils.MoveLastDirectory(path, path);
                    GC.Collect();

                    OnUpdateProgress?.Invoke(1, "Downloaded");
                    OnDownloadComplete?.Invoke(FindAdofaiFiles(path));

                }
                catch (Exception e)
                {
                    ErrorHandler?.Invoke(e);
                }
            });
        }
        


        private static string GetDirectURL(string url, WebClient wc)
        {
            try
            {
                // fix discord
                if (url.Contains("cdn.discordapp.com"))
                    return url.Replace("cdn.discordapp.com","fixcdn.hyonsu.com");
                    
                // google drive file
                if (url.StartsWith("https://drive.google.com/file/d/") ||
                    url.StartsWith("https://drive.google.com/open?id=") ||
                    url.StartsWith("https://drive.google.com/u/0/uc"))
                {
                    if (url.StartsWith("https://drive.google.com/u/0/uc"))
                        return GetDirectURLFromGoogleLargeFile(url, wc);

                    var driveid = "";
                    if (url.Contains("/d/") && url.Contains("/view"))
                        driveid = url.GetValue("/d/", "/view");

                    if (url.Contains("/d/") && url.Contains("/edit"))
                        driveid = url.GetValue("/d/", "/edit");

                    if (url.Contains("id="))
                    {
                        driveid = url.StringSplit("id=")[1];
                        if (driveid.Contains("&"))
                            driveid = driveid.StringSplit("&")[0];
                    }

                    //Main.ModLogger.Log("DriveID: "+driveid);

                    if (driveid == String.Empty)
                        throw new Exception(
                            $"Google Drive id not found\n\n-----Level Info-----\nURL: ${url}");

                    //wc.DownloadData(url);

                    var downloadURL = $"https://drive.google.com/u/0/uc?export=download&id={driveid}";

                    var stream = wc.OpenRead(downloadURL);
                    var buffer = new byte[15];
                    stream.Read(buffer, 0, buffer.Length);
                    stream.Close();
                    
                    //Main.ModLogger.Log(Encoding.UTF8.GetString(buffer));

                    if (Encoding.UTF8.GetString(buffer) == "<!DOCTYPE html>")
                    {
                        //Main.ModLogger.Log(GetDirectURLFromGoogleLargeFile(downloadURL, wc));
                        return GetDirectURLFromGoogleLargeFile(downloadURL, wc);
                    }

                    return downloadURL;
                }

                // mediafire
                if (url.StartsWith("https://www.mediafire.com"))
                {
                    var mfhtml = wc.DownloadString(url);
                    var indexurl = mfhtml.IndexOf("https://download");
                    var indexend = Utils.GetNextIndexOf('"', mfhtml, indexurl);

                    return mfhtml.Substring(indexurl, indexend - indexurl);
                }

                // dropbox
                if (url.StartsWith("https://www.dropbox.com"))
                {
                    var driveid = url.GetValue("https://www.dropbox.com/s/", "?");
                    return $"https://www.dropbox.com/s/{driveid}?dl=1";
                }
    
                if (url.StartsWith("https://drive.google.com/drive/folders/"))
                {
                    throw new Exception(
                        $"Google Drive folder cannot be downloaded\n\n-----Level Info-----\nURL: {url}");
                }

                if (url.StartsWith("https://steamcommunity.com/"))
                {
                    throw new Exception(
                        $"Steam Workshop cannot be downloaded\n\n-----Level Info-----\nURL: {url}");
                }

                return url;
            }
            catch(Exception e)
            {
                throw new Exception(
                    $"The download link is not accessible.\n\n-----Level Info-----\nException: {e.Message}");
            }
        }

        
        private static string GetDirectURLFromGoogleLargeFile(string url, WebClient wc)
        {
            var result = wc.DownloadString(url);
            var id = result.GetValue("name=\"id\" value=\"", "\">");
            var uuid = result.GetValue("name=\"uuid\" value=\"", "\">");
            
            if (!result.Contains("name=\"at\" value=\""))
                return
                    $"https://drive.usercontent.google.com/download?id={id}&export=download&authuser=0&confirm=t&uuid={uuid}";
            var at = result.GetValue("name=\"at\" value=\"", "\">");
            return
                $"https://drive.usercontent.google.com/download?id={id}&export=download&authuser=0&confirm=t&uuid={uuid}&at={at}";

        }
        
        
        private static string GetURLFromLevelID(bool isAdofaiGG, string levelID)
        {
            try
            {
                var wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                if (isAdofaiGG)
                {
                    var html = wc.DownloadString($"https://adofai.gg/api/v1/levels/{levelID}");
                    var download = html.GetValue("\"download\":\"", "\",\"");
                    return download;
                }
                else
                {
                    var html = wc.DownloadString($"https://be.t21c.kro.kr/levels/{levelID}");
                    var download = html.GetValue("\"dlLink\":\"", "\",\"");
                    return download;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"No corresponding levels found on {(isAdofaiGG? "Adofai.gg":"TUC")}.\n\n-----Level Info-----\n"+e);
            }
        }
        
    }
}