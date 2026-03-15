using System.Diagnostics;

namespace PZSaveManager.Classes
{
    public static class WindowHelper
    {
        public static bool IsInForeground()
        {
            return true;
        }

        public static bool IsInForeground(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        public static bool IsInForeground(IntPtr handle)
        {
            return true;
        }

        public static void FlashIfMinimized()
        {
        }

        public static class Buttons
        {
            public static void SetEnabled(IntPtr windowHandle, WindowButton button, bool enabled)
            {
            }

            public static void DisableCloseButton(IntPtr windowHandle)
            {
            }

            public enum WindowButton
            {
                Close,
                Maximize,
                Minimize
            }
        }

        public static class TaskbarProgress
        {
            private static int progress = 0;

            public static TaskbarState State { get; set; } = TaskbarState.NoProgress;

            public static int Progress
            {
                get => progress;
                set
                {
                    if (value < 0 || value > 100)
                        throw new ArgumentOutOfRangeException(nameof(Progress));

                    progress = value;
                }
            }

            public static void Finish()
            {
                Progress = 0;
                State = TaskbarState.NoProgress;
            }

            public enum TaskbarState
            {
                NoProgress,
                Indeterminate,
                Normal,
                Error,
                Paused
            }
        }
    }
}
