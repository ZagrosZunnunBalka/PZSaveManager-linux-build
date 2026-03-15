using Avalonia.Controls;

namespace PZSaveManager.Classes
{
    public static class PageLoader
    {
        public static UserControl? CurrentPage { get; private set; } = null;

        public static void Load<T>(ContentControl host, T page) where T : UserControl, IPage
        {
            CurrentPage = page;
            host.Content = page;
            page.PageLoaded();
        }
    }
}
