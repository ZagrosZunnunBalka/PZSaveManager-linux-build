using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;

namespace PZSaveManager
{
    internal static class Program
    {
        private const string InstanceMutexName = "PZSaveManager_InstanceMutex";

        [STAThread]
        public static void Main(string[] args)
        {
            using var mutex = new Mutex(true, InstanceMutexName, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                Console.WriteLine("Project Zomboid Save Manager is already running.");
                return;
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .LogToTrace();
        }
    }
}
