using System;
using System.Collections;
using System.Collections.Generic;
using DirectLevel;
using UnityEngine;

namespace TUFHelper.ModScripts.Web
{
    public enum LevelDownloaderStates
    {
        Idle,
        Preparing,
        Downloading,
        Unzipping,
        Downloaded,
        Cancelled,
    }

    public class UpdateProgressEventArgs : EventArgs
    {
        public LevelDownloaderStates State { get; set; }
        public long BytesReceived { get; set; } = 0;
        public long TotalBytesToReceive { get; set; } = 0;

        public UpdateProgressEventArgs(LevelDownloaderStates state)
        {
            State = state;
        }
        public UpdateProgressEventArgs(long bytesReceived, long totalBytesToReceive)
        {
            State = LevelDownloaderStates.Downloading;
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }
    }
}
