using System;
using System.IO;
using System.Net;
using System.Text;
using HarmonyLib;

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
        
        internal static void MoveLastDirectory(DirectoryInfo directoryInfo, DirectoryInfo path)
        {
            
            if (directoryInfo.GetDirectories().Length>0)
                MoveLastDirectory(directoryInfo.GetDirectories()[0],path);
            
            if (directoryInfo.GetDirectories().Length < 1)
            {
                if(directoryInfo.FullName==path.FullName) return;
                foreach (var file in directoryInfo.GetFiles())
                {
                    //Logger.Log(file.FullName+"     "+path.FullName+"/"+directoryInfo.Name); 
                    File.Move(file.FullName, path.FullName+"/"+file.Name);
                }
            }
        }
        
        
        internal static int GetNextIndexOf(char c, string source, int start)
        {
            if(start < 0 || start > source.Length - 1)
            {
                return -1;
            }
            for(int i = start; i < source.Length; i++) {
                if(source[i] == c) {
                    return i;
                }
            }
            return -1;
        }
    }
}