using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TUFHelper.Utils
{
    public class LevelFolder
    {
        public List<int> Levels { get; set; }
        public string Name { get; set; }

        public LevelFolder(int[] levels, string name)
        {
            Levels = new(levels);
            Name = name;
        }
    }
}
