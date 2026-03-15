namespace PZSaveManager.Properties
{
    public sealed class Settings
    {
        private static readonly Settings instance = new Settings();

        public static Settings Default => instance;

        public string SavePath { get; set; } = "";

        public bool UseCompression { get; set; } = false;

        public string SaveHotkey { get; set; } = "F5, Control";

        public bool EnableAutosave { get; set; } = false;

        public int AutosaveInterval { get; set; } = 10;

        public string AbortSaveHotkey { get; set; } = "F6, Control";

        public bool UseSaveSounds { get; set; } = true;

        public int SoundVolume { get; set; } = 50;

        public bool AutoCheckForUpdates { get; set; } = true;

        public bool MinimizeToSystemTray { get; set; } = true;

        public bool IsEverMinimized { get; set; } = false;

        public string DuserPath { get; set; } = "";

        private Settings() { }
    }
}
