using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TUFHelper;

namespace DirectLevel
{
    public class DirectLevelAPI
    {
        public enum ForumType
        {
            ADOFAI_GG,
            TUC
        }

        public static bool IsDownloading;
        
        public static void DownloadLevel(string url, string downloadDirectory)
        {
            var wc = new DownloadManager.CookieWebClient();
            wc.Encoding = Encoding.UTF8;

            var urlId = url.GetHashCode();
            var downloadURL = DownloadManager.GetDirectURL(url, wc);
            var directoryInfo2 = new DirectoryInfo(Path.Combine(downloadDirectory, urlId.ToString()));
            var fileInfo = new FileInfo($"{Path.Combine(downloadDirectory, urlId.ToString())}.zip");
            
            if (fileInfo.Exists)
            {
                File.Delete(fileInfo.FullName);
                
                if(directoryInfo2.Exists && directoryInfo2.GetFiles().Length > 0)
                    Directory.Delete(directoryInfo2.FullName, true);
                
                DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }

            if (directoryInfo2.Exists && directoryInfo2.GetFiles().Length == 0)
            {
                DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }

            if (directoryInfo2.Exists)
            {
                DownloadPopupScript.ChangeProgress = 3 / 6f;
                return;
            }
            directoryInfo2.Create();
            
            DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
        }
        
        
        
        public static void PlayFromID(ForumType forumType, string id, bool openAtEditor, string containName = null, bool cache = false)
        {
            IsDownloading = true;
            
            
            GC.Collect();

            DownloadPopupScript.ChangeMessage = "Parsing";
            DownloadPopupScript.ChangeProgress = 1 / 6f;
            
            var url = DownloadManager.GetURLFromLevelID(forumType == ForumType.ADOFAI_GG, id);
            var urlId = url.GetHashCode();
            var directoryInfo = new DirectoryInfo(Main.Setting.levelSaveFolder);
            var path = Path.Combine(directoryInfo.FullName, urlId.ToString());
            if(!directoryInfo.Exists) directoryInfo.Create();
            
            DownloadPopupScript.ChangeMessage = "Downloading";
            DownloadPopupScript.ChangeProgress = 2 / 6f;
            
            DownloadLevel(url, directoryInfo.FullName);
            DownloadManager.PlayLevel(path, openAtEditor, containName);

            IsDownloading = false;

            if (!cache)
                Main.removeLevels.Add(path);
        }
        
        
        //Test
        public static Task PlayFromIDTask(ForumType forumType, string id, bool openAtEditor, string containName, bool cache, Action<Exception> errorHandler)
        {
            return Task.Run(() =>
            {
                try
                {
                    PlayFromID(forumType, id, openAtEditor, containName, cache);
                }
                catch (Exception e)
                {
                    errorHandler?.Invoke(e);
                }
            });
        }


        public static void Play(string url, bool openAtEditor, string containName = null, bool cache = false)
        {
            
            var urlId = url.GetHashCode();
            var directoryInfo = new DirectoryInfo(Main.Setting.levelSaveFolder);
            var path = Path.Combine(directoryInfo.FullName, urlId.ToString());
            if(!directoryInfo.Exists) directoryInfo.Create();
            
            DownloadLevel(url, directoryInfo.FullName);
            DownloadManager.PlayLevel(path, openAtEditor,containName);

            if (!cache)
                Main.removeLevels.Add(path);
            
            
        }
    }
}