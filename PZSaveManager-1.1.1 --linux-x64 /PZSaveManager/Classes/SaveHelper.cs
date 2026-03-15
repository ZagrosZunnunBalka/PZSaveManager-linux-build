using PZSaveManager.Properties;
using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace PZSaveManager.Classes
{
    public static class SaveHelper
    {
        public const int DefaultAutosaveInterval = 10;

        private static Timer autosaveTimer = new();
        private static CancellationTokenSource token = new();

        public static void UpdateAutosaveTimer()
        {
            autosaveTimer.Elapsed -= AutosaveTimer_Elapsed;
            autosaveTimer.Stop();

            if (Settings.Default.EnableAutosave)
            {
                if (Settings.Default.AutosaveInterval <= 0)
                {
                    Settings.Default.AutosaveInterval = DefaultAutosaveInterval;
                    Logger.Log("The auto-save interval was invalid and has been reset.", LogSeverity.Warning);
                }

                autosaveTimer = new Timer
                {
                    Interval = Settings.Default.AutosaveInterval * 60_000
                };

                autosaveTimer.Elapsed += AutosaveTimer_Elapsed;
                autosaveTimer.Start();

                Logger.Log($"Auto-save enabled ({Settings.Default.AutosaveInterval} minutes).", LogSeverity.Info);
            }
            else
            {
                Logger.Log("Auto-save disabled.", LogSeverity.Info);
            }
        }

        private static async Task PerformSave(string description)
        {
            Logger.Log($"{description} requested.", LogSeverity.Info);

            if (!Save.IsSaveInProgress)
            {
                token = new CancellationTokenSource();

                // TODO: World.SaveActiveWorldAsync yeniden implement edilmeli
                Logger.Log("Save system not yet implemented in Avalonia version.", LogSeverity.Warning);

                await Task.CompletedTask;
            }
            else
            {
                Logger.Log("Another save already running.", LogSeverity.Warning);
                SoundPlayer.Shared.PlaySaveEffect(SoundEffect.AlreadySaving);
            }
        }

        public static void AbortSave()
        {
            if (Save.IsSaveInProgress)
            {
                if (Save.IsSaveCancelable)
                {
                    token.Cancel();
                    Logger.Log("Save cancellation requested.", LogSeverity.Info);
                }
                else
                {
                    Logger.Log("Save cannot be aborted anymore.", LogSeverity.Warning);
                    SoundPlayer.Shared.PlaySaveEffect(SoundEffect.SaveNotCanceled);
                }
            }
            else
            {
                Logger.Log("No save in progress.", LogSeverity.Info);
            }
        }

        public static void Dispose()
        {
            autosaveTimer.Dispose();
            token.Dispose();
        }

        private static async void AutosaveTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            await PerformSave(Save.AutosaveDescription);
        }
    }
}
