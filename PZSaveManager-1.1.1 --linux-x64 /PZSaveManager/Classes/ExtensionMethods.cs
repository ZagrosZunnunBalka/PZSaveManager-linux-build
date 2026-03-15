using System;
using System.IO;
using System.Threading.Tasks;

namespace PZSaveManager.Classes
{
    public static class ExtensionMethods
    {
        public static async Task DeleteParallelAsync(this DirectoryInfo path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!path.Exists)
                return;

            await Task.Run(() =>
            {
                Parallel.ForEach(
                    Directory.EnumerateFiles(path.FullName, "*", SearchOption.AllDirectories),
                                 filePath =>
                                 {
                                     try
                                     {
                                         File.Delete(filePath);
                                     }
                                     catch (Exception ex)
                                     {
                                         Logger.Log($"Could not delete {filePath}", ex);
                                     }
                                 });

                try
                {
                    if (Directory.Exists(path.FullName))
                        Directory.Delete(path.FullName, true);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Could not delete directory {path.FullName}", ex);
                }
            });
        }
    }
}
