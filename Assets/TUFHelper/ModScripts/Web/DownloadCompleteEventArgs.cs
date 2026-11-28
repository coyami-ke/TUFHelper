using DirectLevel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TUFHelper.ModScripts.Web
{
    public class DownloadCompleteEventArgs : EventArgs
    {
        public List<LevelDownloader.DownloadedLevel> Levels { get; set; }

        public DownloadCompleteEventArgs(List<LevelDownloader.DownloadedLevel> levels)
        {
            Levels = levels;
        }
    }
}