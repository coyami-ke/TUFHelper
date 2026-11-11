using System;
using System.Globalization;
using System.IO;
using System.Security;

namespace TUFHelper.Integration
{
    /// <summary>
    /// Lets other mods tell which TUF level a chart downloaded by TUFHelper belongs to.
    /// Other mods find this method by name through reflection, so keep the type name,
    /// method name, and signature stable:
    /// <c>TUFHelper.Integration.LevelContextResolver.ResolveTufLevelId(string) -> int?</c>.
    /// TUFHelperLite exposes the same method.
    /// </summary>
    public static class LevelContextResolver
    {
        private static readonly char[] DirectorySeparators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        /// <summary>
        /// Returns the TUF level ID when <paramref name="levelPath"/> is an existing .adofai file
        /// inside a level folder in TUFHelper's level save folder; otherwise null.
        /// Uses no Unity API, so it can be called from any thread.
        /// </summary>
        public static int? ResolveTufLevelId(string levelPath)
        {
            if (string.IsNullOrWhiteSpace(levelPath) ||
                !string.Equals(Path.GetExtension(levelPath), ".adofai", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string saveFolder = Main.Setting?.LevelSaveFolder;
            if (string.IsNullOrWhiteSpace(saveFolder)) return null;

            try
            {
                string fullLevelPath = Path.GetFullPath(levelPath);
                if (!File.Exists(fullLevelPath)) return null;

                string relativePath = Path.GetRelativePath(Path.GetFullPath(saveFolder), fullLevelPath);
                if (string.IsNullOrEmpty(relativePath) || Path.IsPathRooted(relativePath)) return null;

                int separatorIndex = relativePath.IndexOfAny(DirectorySeparators);
                if (separatorIndex <= 0) return null;

                string levelFolderName = relativePath.Substring(0, separatorIndex);
                if (levelFolderName == "..") return null;

                return TryParseLevelFolderName(levelFolderName, out int levelId) ? levelId : (int?)null;
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is IOException ||
                exception is NotSupportedException ||
                exception is SecurityException ||
                exception is UnauthorizedAccessException)
            {
                return null;
            }
        }

        // LevelDownloader.GetPathToLevelFolder names level folders "{artist} - {song} {id}".
        private static bool TryParseLevelFolderName(string folderName, out int levelId)
        {
            levelId = 0;
            int lastSpace = folderName.LastIndexOf(' ');
            int artistEnd = folderName.IndexOf(" - ", StringComparison.Ordinal);
            return artistEnd > 0 &&
                artistEnd + 3 < lastSpace &&
                int.TryParse(
                    folderName.Substring(lastSpace + 1),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out levelId) &&
                levelId > 0;
        }
    }
}
