using System;
using System.IO;
using System.Text;
using TUFHelper;

namespace DirectLevel
{
    public class DirectLevelAPI
    {
        public enum ForumType
        {
            ADOFAI_GG,
            T21C
        }
        
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
                DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }

            if (directoryInfo2.Exists && directoryInfo2.GetFiles().Length == 0)
            {
                DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }
            
            if (directoryInfo2.Exists) return;
            directoryInfo2.Create();
            
            DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
        }


        public static void DownloadLevelFromID(ForumType forumType, string id, string downloadDirectory)
        {
            var wc = new DownloadManager.CookieWebClient();
            wc.Encoding = Encoding.UTF8;

            var url = DownloadManager.GetURLFromLevelID(forumType == ForumType.ADOFAI_GG, id);
            var urlId = url.GetHashCode();
            var downloadURL = DownloadManager.GetDirectURL(url, wc);
            var directoryInfo2 = new DirectoryInfo(Path.Combine(downloadDirectory, urlId.ToString()));
            var fileInfo = new FileInfo($"{Path.Combine(downloadDirectory, urlId.ToString())}.zip");
            if (fileInfo.Exists)
            {
                File.Delete(fileInfo.FullName);
                DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }

            if (directoryInfo2.Exists && directoryInfo2.GetFiles().Length == 0)
            {
                DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
                return;
            }
            
            if (directoryInfo2.Exists) return;
            directoryInfo2.Create();
            
            DownloadManager.Download(downloadURL, directoryInfo2.FullName, wc);
        }
        
        
        public static void PlayFromID(ForumType forumType, string id, bool openAtEditor, bool cache = false)
        {
            LevelLoadPatch.IsLoadDirectLevel = true;
            LevelLoadPatch.IsLoading = true;
            
            var url = DownloadManager.GetURLFromLevelID(forumType == ForumType.ADOFAI_GG, id);
            var urlId = url.GetHashCode();
            var directoryInfo = new DirectoryInfo(Main.Setting.levelSaveFolder);
            var path = Path.Combine(directoryInfo.FullName, urlId.ToString());
            if(!directoryInfo.Exists) directoryInfo.Create();
            
            DownloadLevel(url, directoryInfo.FullName);
            DownloadManager.PlayLevel(path, openAtEditor);

            if (!cache)
                LevelLoadPatch.RemoveLevels.Add(path);
            
        }


        public static void Play(string url, bool openAtEditor, bool cache = false)
        {
            LevelLoadPatch.IsLoadDirectLevel = true;
            LevelLoadPatch.IsLoading = true;
            
            var urlId = url.GetHashCode();
            var directoryInfo = new DirectoryInfo(Main.Setting.levelSaveFolder);
            var path = Path.Combine(directoryInfo.FullName, urlId.ToString());
            if(!directoryInfo.Exists) directoryInfo.Create();
            
            DownloadLevel(url, directoryInfo.FullName);
            DownloadManager.PlayLevel(path, openAtEditor);

            if (!cache)
                LevelLoadPatch.RemoveLevels.Add(path);
            
            
        }
    }
}