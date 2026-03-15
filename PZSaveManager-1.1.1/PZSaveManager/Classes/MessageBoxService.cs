using System;
using System.Threading.Tasks;

namespace PZSaveManager.Classes
{
    public static class MessageBoxService
    {
        public static Task ShowError(string text)
        {
            Console.WriteLine("ERROR: " + text);
            return Task.CompletedTask;
        }

        public static Task ShowWarning(string text)
        {
            Console.WriteLine("WARNING: " + text);
            return Task.CompletedTask;
        }

        public static Task ShowInfo(string text, string title)
        {
            Console.WriteLine($"{title}: {text}");
            return Task.CompletedTask;
        }

        public static Task<bool> ShowConfirmation(string text, string title, bool isYesDefault = false)
        {
            Console.WriteLine($"{title}: {text}");
            return Task.FromResult(true);
        }
    }
}
