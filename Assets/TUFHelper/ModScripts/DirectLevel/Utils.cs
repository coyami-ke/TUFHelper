using System;
using System.IO;
using System.Net;
using System.Text;
using HarmonyLib;
using TUFHelper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DirectLevel
{
    internal static class Utils
    {
        
        internal static string[] StringSplit(this string org, string str1)
        {
            return org.Split(new[] { str1 }, StringSplitOptions.None);
        }

        internal static string GetValue(this string org, string str1, string str2)
        {
            return org.StringSplit(str1)[1]?.StringSplit(str2)[0]?.Trim();
        }
        
        internal static void MoveLastDirectory(string startPath, string toPath)
        {

            var directories = Directory.GetDirectories(startPath);
            if (directories.Length > 0)
            {
                foreach (var t in directories)
                    MoveLastDirectory(t, toPath);
                
            } else {
                if(startPath == toPath) return;
                foreach (var file in Directory.GetFiles(startPath))
                {
                    var moveToPath = Path.Combine(toPath, Path.GetFileName(file));
                    if (File.Exists(moveToPath)) continue;
                    File.Move(file, moveToPath);
                }
                
            }
        }


        internal static void RunAtMainThread(Action action)
        {
            if (Main.mainThread == null) return;
            Main.mainThread.Post(_ => { action?.Invoke(); }, null);
        }
        
        public static long GetFileSize(string url)
        {
            var result = 0L;

            var req = WebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long contentLength))
                {
                    result = contentLength;
                }
            }

            return result;
        }

        internal static string ByteToStringUnit(long byteNum)
        {
            var kb = byteNum * 0.001f;
            if (kb < 1)
                return byteNum.ToString();
            
            if (kb < 1000 && kb >= 1)
                return $"{kb}KB";
            
            var mb = kb * 0.001f;
            if (mb < 1000 && kb >= 1)
                return $"{mb}MB";
            
            var gb = mb * 0.001f;
            return $"{gb}GB";

        }
        
        
        internal static int GetNextIndexOf(char c, string source, int start)
        {
            if(start < 0 || start > source.Length - 1)
            {
                return -1;
            }
            for(var i = start; i < source.Length; i++) {
                if(source[i] == c) {
                    return i;
                }
            }
            return -1;
        }
    }
}