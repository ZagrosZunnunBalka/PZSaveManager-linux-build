using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace PZSaveManager.Classes
{
    public static class GameScreen
    {
        private const string Game32ProcessName = "javaw";
        private const string Game64ProcessName = "ProjectZomboid64";

        public static Bitmap? Capture()
        {
            if (!WindowHelper.IsInForeground(Game32ProcessName) &&
                !WindowHelper.IsInForeground(Game64ProcessName))
                return null;

            string outputPath = Path.Combine(Path.GetTempPath(), $"pzsave_{Guid.NewGuid()}.png");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "scrot",
                    Arguments = $"\"{outputPath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit();

                if (!File.Exists(outputPath))
                {
                    Logger.Log("scrot çalıştı fakat dosya oluşmadı.");
                    return null;
                }

                var bitmap = new Bitmap(outputPath);

                try
                {
                    File.Delete(outputPath);
                }
                catch { }

                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.Log("Ekran yakalama başarısız.", ex);
                return null;
            }
        }
    }
}
