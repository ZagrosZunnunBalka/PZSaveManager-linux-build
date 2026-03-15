using Avalonia.Input;

namespace PZSaveManager.Classes
{
    public interface IPage
    {
        void PageLoaded();
        void UpdateUI();
        void GlobalKeyDown(Key key);
    }
}
