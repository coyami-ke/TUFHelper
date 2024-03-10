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
        

        public static void Download(string url, string path, WebClient wc)
        {
            var stream = wc.OpenRead(url);
                
            var buffer = new byte[15];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            
            var isntDownloadFile = Encoding.UTF8.GetString(buffer) == "<!DOCTYPE html>";

            if (isntDownloadFile)
            {
                if (url.Contains("google.com"))
                    throw new Exception("This drive file is temporarily unavailable for download.");
                
                throw new Exception("This URL is not a download file.");
            }


            var zipPath = $"{path}.zip";
            wc.DownloadProgressChanged += (sender, args) =>
            { 
                DownloadPopupScript.ChangeMessage = $"Downloading... ({args.BytesReceived}/{args.TotalBytesToReceive})";
            };
            DownloadPopupScript.ChangeProgress = 3 / 6f;
            var t = wc.DownloadFileTaskAsync(url, zipPath); 
            t.Wait();
            
            DownloadPopupScript.ChangeMessage = "Unzipping";
            DownloadPopupScript.ChangeProgress = 4 / 6f;
            
            ZipHelper.Unzip(zipPath, path);
            File.Delete(zipPath);
        }
        

        internal static void PlayLevel(string path, bool toEditor, string containName)
        {
            DownloadPopupScript.ChangeProgress = 5 / 6f;
            DownloadPopupScript.ChangeMessage = "Searching main level file";
            
            var loadPath = "";
            var containLoadPath = "";

            var di = new DirectoryInfo(path + "/");
            Utils.MoveLastDirectory(di, di);

            var containMapSize = 0L;
            var mapSize = 0L;
            var songSize = 0L;

            string ogg = null;
            foreach (var file in di.GetFiles())
            {
                
                // find main song file
                if (ogg == null)
                {
                    if (file.Extension.ToLower().Contains("ogg") && file.Length > songSize)
                    {
                        ogg = file.FullName;
                        songSize = file.Length;
                    }
                }
                
                
                //find main adofai file
                if (!file.Extension.Contains("adofai")) continue;
                if (file.Name.Contains("backup")) continue;
                
                if (file.Name.ToLower().Contains("main"))
                {
                    loadPath = file.FullName;
                    break;
                }
                
                if (!string.IsNullOrEmpty(containName))
                {
                    if (containName.Contains(containName))
                    {
                        if (file.Length > containMapSize)
                        {
                            containMapSize = file.Length;
                            containLoadPath = file.FullName;
                        }
                    }
                }

                if (file.Length > mapSize)
                {
                    mapSize = file.Length;
                    loadPath = file.FullName;
                }
                
            }
            
            if (!string.IsNullOrEmpty(containLoadPath))
                loadPath = containLoadPath;

            
            /*
            if (!string.IsNullOrEmpty(ogg))
                LevelLoadPatch.SongPath = ogg;*/

            GC.Collect();
            GCS.checkpointNum = 0;
            
            //run at mainThread
            DownloadPopupScript.ChangeMessage = "Opening a Level...";
            DownloadPopupScript.ChangeProgress = 1;
            
            Main.mainThread.Post(_ =>
            {
                if (scrController.instance != null)
                {
                    if (toEditor)
                    {
                        GCS.sceneToLoad = "scnEditor";
                        SceneManager.LoadScene("scnEditor");
                        scnEditor.levelToOpenOnLoad = loadPath;
                        scrController.instance.StartLoadingScene();
                    }
                    else
                    {
                        scrController.instance.LoadCustomLevel(loadPath);
                    }
                }
                else
                {
                    void Invoke()
                    {
                        Patch.RecentDirectLevelOpend = true;
                        
                        if (toEditor)
                        {
                            GCS.sceneToLoad = "scnEditor";
                            scnEditor.levelToOpenOnLoad = loadPath;
                            GCS.worldEntrance = null;
                            SteamIntegration.EditorEntered();
                            SceneManager.LoadScene("scnEditor");
                        }
                        else
                        {
                            GCS.sceneToLoad = "scnGame";
                            SceneManager.LoadScene("scnGame");
                            GCS.customLevelPaths = new string[1];
                            GCS.customLevelPaths[0] = loadPath;
                        }
                        
                    }

                    if (SceneManager.GetActiveScene().name == "TUFLevelSelect") 
                        UIScript.SwipeToBlack(Invoke);
                    else
                        Invoke();
                    
                }
            }, null);
            
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
                            $"Google Drive id not found\n\n-----Level Info-----\nURL: ${url}");

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
                throw new Exception($"No corresponding levels found on {(isAdofaiGG? "Adofai.gg":"TUC")}.\n\n-----Level Info-----\n"+e);
            }
        }
    }
}