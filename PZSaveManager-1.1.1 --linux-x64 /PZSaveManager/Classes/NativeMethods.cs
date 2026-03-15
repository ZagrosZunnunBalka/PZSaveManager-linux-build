using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace PZSaveManager.Classes
{
    public static class NativeMethods
    {
        private static Window? MainWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
        ?.MainWindow;

        public static void FlashWindow(IntPtr hwnd)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var win = MainWindow;
                if (win is null) return;
                if (!win.IsActive)
                {
                    win.ShowInTaskbar = true;
                    win.Activate();
                }
            });
        }

        public static IntPtr GetForegroundWindow()
        {
            var win = MainWindow;
            if (win?.IsActive == true)
                return new IntPtr(1);
            return IntPtr.Zero;
        }

        public static IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert) => IntPtr.Zero;
        public static bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags) => false;
        public static bool EnableMenuItem(IntPtr hMenu, uint itemId, uint uEnable) => false;

        public static long GetWindowLong(IntPtr hwnd, int nIndex)
        {
            var win = MainWindow;
            if (win is null) return (long)WindowStyleFlag.Normal;

            long flags = (long)WindowStyleFlag.Normal;
            if (win.WindowState == WindowState.Maximized)
                flags |= (long)WindowStyleFlag.Maximized;
            if (win.WindowState == WindowState.FullScreen)
                flags |= (long)WindowStyleFlag.Fullscreen;
            if (win.WindowState == WindowState.Minimized || !win.IsVisible)
                flags |= (long)WindowStyleFlag.Hidden;
            return flags;
        }

        public static void SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var win = MainWindow;
                if (win is null) return;

                if ((dwNewLong & (long)WindowStyleFlag.Fullscreen) != 0)
                    win.WindowState = WindowState.FullScreen;
                else if ((dwNewLong & (long)WindowStyleFlag.Maximized) != 0)
                    win.WindowState = WindowState.Maximized;
                else if ((dwNewLong & (long)WindowStyleFlag.Hidden) != 0)
                    win.WindowState = WindowState.Minimized;
                else
                    win.WindowState = WindowState.Normal;
            });
        }
    }

    public enum WindowStyleFlag : long
    {
        Normal     = 0,
        Maximized  = 1 << 0,
        Fullscreen = 1 << 1,
        Hidden     = 1 << 2
    }
}
