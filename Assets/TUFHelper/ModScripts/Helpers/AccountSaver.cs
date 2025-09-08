using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace TUFHelper.Utils
{
    public class AccountSaver
    {
        public static readonly string PATH_TO_ACCOUNT_FILE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TUFHelper", "token.json");

        public string Token { get; set; } = "";
        public bool IsRatingMode { get; set; } = false;

        public static AccountSaver GetAccount()
        {
            if (!File.Exists(PATH_TO_ACCOUNT_FILE)) return null;
            return JsonConvert.DeserializeObject<AccountSaver>(File.ReadAllText(PATH_TO_ACCOUNT_FILE));
        }

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(PATH_TO_ACCOUNT_FILE)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PATH_TO_ACCOUNT_FILE));
            }
            File.WriteAllText(PATH_TO_ACCOUNT_FILE, JsonConvert.SerializeObject(this));
        }
    }
}