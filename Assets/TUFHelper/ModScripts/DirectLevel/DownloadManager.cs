using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using TUFHelper;
using TUFHelper.Utils;
using UnityEngine.SceneManagement;

namespace DirectLevel
{
    public class DownloadManager
    {
        
        public class CookieWebClient : WebClient
        {
            CookieContainer c = new CookieContainer();

            protected override WebRequest GetWebRequest(Uri u)
            {
                var r = (HttpWebRequest)base.GetWebRequest(u);
                r.CookieContainer = c;
                return r;
            }
        }

        private static string currentID;
        
        public static void DownloadLevel(bool isAdofaiGG, string levelID)
        {
            currentID = levelID;
            var directoryInfo = new DirectoryInfo(Main.Setting.levelSaveFolder);
            if(!directoryInfo.Exists) directoryInfo.Create();

            var wc = new CookieWebClient();
            wc.Encoding = Encoding.UTF8;
            
            var downloadURL = GetDirectURL(GetURLFromLevelID(isAdofaiGG, levelID), wc);
            var directoryInfo2 = new DirectoryInfo(Main.Setting.levelSaveFolder  + $"/{levelID}{(isAdofaiGG?"A":"T")}");
            var directoryInfo3 = new FileInfo(Main.Setting.levelSaveFolder + $"/{levelID}{(isAdofaiGG?"A":"T")}.zip");
            if (directoryInfo3.Exists)
            {
                File.Delete(directoryInfo3.FullName);
                
                Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }

            if (directoryInfo2.Exists && directoryInfo2.GetFiles().Length == 0)
            {
                Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }
            
            if (directoryInfo2.Exists) return;
            directoryInfo2.Create();
            
            Download(downloadURL, directoryInfo2.FullName, wc);
            
        }

        public static void Download(string url, string path, WebClient wc)
        {
            var zipPath = $"{path}.zip";
            wc.DownloadFile(url, zipPath);
            
            ZipHelper.Unzip(zipPath, path);
            File.Delete(zipPath);
        }
        
        /*private static void Unzip(string zipFilePath, string location)
        {
            //Directory.CreateDirectory(location);
            //ZipFile.ExtractToDirectory(zipFilePath, location);
            
            //var encoding = System.Text.Encoding.GetEncoding("da-DK");
            

            if (zipFilePath == null) return;

            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFilePath, ZipArchiveMode.Read, Encoding.UTF8))
            {
                zipArchive.ExtractToDirectory(location);
            }
            
            //ZipUtils.Unzip(zipFilePath, location);
        }*/

        internal static void PlayLevel(string path, bool toEditor)
        {
            var loadpath = "";

            var di = new DirectoryInfo(path + "/");
            Utils.MoveLastDirectory(di, di);
            var size = 0L;
            var heavySize = 0L;

            string ogg = null;
            foreach (var file in di.GetFiles())
            {
                if (ogg == null)
                {
                    if (file.Extension.ToLower().Contains("ogg") && file.Length > heavySize)
                    {
                        ogg = file.FullName;
                        heavySize = file.Length;
                    }
                }
                
                if (!file.Extension.Contains("adofai")) continue;
                if (file.Name.Contains("backup")) continue;
                if (file.Name.ToLower().Contains("main"))
                {
                    loadpath = file.FullName;
                    break;
                }

                if (file.Length > size)
                {
                    size = file.Length;
                    loadpath = file.FullName;
                }
            }

            if (!string.IsNullOrEmpty(ogg))
            {
                /*
                if (!File.Exists(Path.Combine(di.FullName, "song.ogg")))
                    File.Move(ogg, Path.Combine(di.FullName, "song.ogg"));
                Patch.SongPath = Path.Combine(di.FullName, "song.ogg");*/
                
                LevelLoadPatch.SongPath = ogg;
            }


            GCS.checkpointNum = 0;
            if (scrController.instance != null)
            {
                if (toEditor)
                {
                    GCS.sceneToLoad = "scnEditor";
                    SceneManager.LoadScene("scnEditor");
                    scnEditor.levelToOpenOnLoad = loadpath;
                    scrController.instance.StartLoadingScene();
                }
                else
                {
                    scrController.instance.LoadCustomLevel(loadpath);
                }
            }
            else
            {
                if (toEditor)
                {
                    GCS.sceneToLoad = "scnEditor";
                    SceneManager.LoadScene("scnEditor");
                    scnEditor.levelToOpenOnLoad = loadpath;
                }
                else
                {
                    GCS.sceneToLoad = "scnGame";
                    SceneManager.LoadScene("scnGame");
                    GCS.customLevelPaths = new string[1];
                    GCS.customLevelPaths[0] = loadpath;
                }
            }
        }
        
        internal static string GetDirectURL(string url, WebClient wc)
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
                            $"Google Drive id not found\n\n-----Level Info-----\nLevelID: {currentID}\nURL: ${url}");

                    wc.DownloadData(url);

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
                        $"Google Drive folder cannot be downloaded\n\n-----Level Info-----\nLevelID: {currentID}\nURL: {url}");
                }

                if (url.StartsWith("https://steamcommunity.com/"))
                {
                    throw new Exception(
                        $"Steam Workshop cannot be downloaded\n\n-----Level Info-----\nLevelID: {currentID}\nURL: {url}");
                }

                return url;
            }
            catch(Exception e)
            {
                throw new Exception(
                    $"The download link is not accessible.\n\n-----Level Info-----\nLevelID: {currentID}\nURL: {url}\nException: {e.Message}");
            }
        }

        private static string GetDirectURLFromGoogleLargeFile(string url, WebClient wc)
        {
            var result = wc.DownloadString(url);
            //Main.ModLogger.Log(result);
            var id = result.GetValue("name=\"id\" value=\"", "\">");
            //Main.ModLogger.Log(id);
            var uuid = result.GetValue("name=\"uuid\" value=\"", "\">");
            //Main.ModLogger.Log(uuid);
            if (result.Contains("name=\"at\" value=\""))
            {
                var at = result.GetValue("name=\"at\" value=\"", "\">");
                return
                    $"https://drive.usercontent.google.com/download?id={id}&export=download&authuser=0&confirm=t&uuid={uuid}&at={at}";
            }

            return
                $"https://drive.usercontent.google.com/download?id={id}&export=download&authuser=0&confirm=t&uuid={uuid}";
        }

        internal static string GetURLFromLevelID(bool isAdofaiGG, string levelID)
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
                throw new Exception($"No corresponding levels found on Adofai.gg.\n\n-----Level Info-----\nLevelID: {currentID}\n"+e);
            }
        }
    }
}