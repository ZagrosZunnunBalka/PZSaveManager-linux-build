using System;
using System.Diagnostics;
using System.IO;

namespace PZSaveManager.Classes
{
    public static class FileExplorer
    {
        public static bool Browse(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Logger.Log("Browse called with null or empty path.");
                return false;
            }

            try
            {
                // Eğer dosya verilmişse klasörünü aç
                string target = File.Exists(path)
                ? Path.GetDirectoryName(path) ?? path
                : path;

                Process.Start("xdg-open", target)?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not browse {path}", ex);
                return false;
            }

            return true;
        }
    }
}
