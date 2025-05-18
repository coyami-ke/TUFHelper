using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TUFHelper.ModScripts.Web
{
    public class DownloadCompleteEventArgs : EventArgs
    {
        public List<string> Levels { get; set; }

        public DownloadCompleteEventArgs(List<string> levels)
        {
            Levels = levels;
        }
    }
}