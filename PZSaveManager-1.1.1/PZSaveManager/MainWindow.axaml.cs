using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PZSaveManager.Classes;

namespace PZSaveManager
{
    public partial class MainWindow : Window
    {
        private ListBox? worldList;
        private ListBox? backupList;
        private ListBox? logList;

        private ProgressBar? progressBar;
        private TextBox? metadataBox;
        private TextBox? backupNameBox;

        // NEW UI ELEMENTS
        private TextBox? backupSearchBox;

        private List<World> worlds = new();
        private List<Save> currentBackups = new();

        public MainWindow()
        {
            InitializeComponent();

            worldList = this.FindControl<ListBox>("WorldList");
            backupList = this.FindControl<ListBox>("BackupList");
            logList = this.FindControl<ListBox>("LogList");

            metadataBox = this.FindControl<TextBox>("MetadataBox");
            progressBar = this.FindControl<ProgressBar>("ArchiveProgressBar");
            backupNameBox = this.FindControl<TextBox>("BackupNameBox");

            // NEW
            backupSearchBox = this.FindControl<TextBox>("BackupSearchBox");

            var reloadButton = this.FindControl<Button>("ReloadButton");
            var backupButton = this.FindControl<Button>("BackupButton");
            var restoreButton = this.FindControl<Button>("RestoreButton");
            var deleteButton = this.FindControl<Button>("DeleteButton");
            var openFolderButton = this.FindControl<Button>("OpenFolderButton");

            // NEW
            var renameButton = this.FindControl<Button>("RenameButton");

            if (reloadButton != null) reloadButton.Click += ReloadClicked;
            if (backupButton != null) backupButton.Click += BackupClicked;
            if (restoreButton != null) restoreButton.Click += RestoreClicked;
            if (deleteButton != null) deleteButton.Click += DeleteClicked;
            if (openFolderButton != null) openFolderButton.Click += OpenFolderClicked;

            // NEW
            if (renameButton != null) renameButton.Click += RenameClicked;

            if (worldList != null)
                worldList.SelectionChanged += WorldChanged;

            if (backupList != null)
                backupList.SelectionChanged += BackupChanged;

            // NEW
            if (backupSearchBox != null)
                backupSearchBox.KeyUp += BackupSearchChanged;

            Logger.Logged += OnLogReceived;

            LoadWorlds();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // ------------------------------------------------
        // LOGGER
        // ------------------------------------------------

        private void OnLogReceived(object? sender, LogEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (logList == null)
                    return;

                logList.Items.Add(e.Message);

                if (logList.ItemCount > 0)
                    logList.ScrollIntoView(logList.Items[^1]);
            });
        }

        // ------------------------------------------------
        // WORLD LIST
        // ------------------------------------------------

        private void LoadWorlds()
        {
            if (worldList == null)
                return;

            worldList.Items.Clear();

            worlds = World.GetAllWorlds().ToList();

            foreach (var world in worlds)
                worldList.Items.Add(world.Name);

            Logger.Log("World list loaded.");

            if (worldList.ItemCount > 0)
                worldList.SelectedIndex = 0;
        }

        private World? GetSelectedWorld()
        {
            if (worldList == null)
                return null;

            int index = worldList.SelectedIndex;

            if (index < 0 || index >= worlds.Count)
                return null;

            return worlds[index];
        }

        // ------------------------------------------------
        // BACKUP LIST
        // ------------------------------------------------

        private void LoadBackups(World world)
        {
            if (backupList == null)
                return;

            currentBackups = world
            .GetSaves()
            .OrderByDescending(s => s.Date)
            .ToList();

            backupList.Items.Clear();

            foreach (var save in currentBackups)
                backupList.Items.Add(save.Description);

            Logger.Log($"Loaded {currentBackups.Count} backups for world {world.Name}");
        }

        private Save? GetSelectedBackup()
        {
            if (backupList == null)
                return null;

            int index = backupList.SelectedIndex;

            if (index < 0 || index >= currentBackups.Count)
                return null;

            return currentBackups[index];
        }

        // ------------------------------------------------
        // WORLD SELECTION
        // ------------------------------------------------

        private void WorldChanged(object? sender, SelectionChangedEventArgs e)
        {
            var world = GetSelectedWorld();

            if (world == null)
                return;

            LoadBackups(world);

            if (metadataBox != null)
                metadataBox.Text = "";
        }

        // ------------------------------------------------
        // BACKUP SELECTION
        // ------------------------------------------------

        private void BackupChanged(object? sender, SelectionChangedEventArgs e)
        {
            var save = GetSelectedBackup();

            if (save == null)
                return;

            ShowMetadata(save);
        }

        // ------------------------------------------------
        // METADATA
        // ------------------------------------------------

        private void ShowMetadata(Save save)
        {
            if (metadataBox == null)
                return;

            try
            {
                if (save.ArchivePath == null)
                    return;

                string metadataPath = Path.Combine(
                    Directory.GetParent(save.ArchivePath)!.FullName,
                                                   Save.MetadataFileName
                );

                if (!File.Exists(metadataPath))
                {
                    metadataBox.Text = "Metadata not found.";
                    return;
                }

                var doc = XDocument.Load(metadataPath);

                var worldName = doc.Root?.Element("WorldName")?.Value;
                var gamemode = doc.Root?.Element("WorldGamemode")?.Value;
                var description = doc.Root?.Element("Description")?.Value;
                var date = doc.Root?.Element("Date")?.Value;

                // NEW SIZE INFO
                string size = GetBackupSize(save);

                metadataBox.Text =
                $@"World: {worldName}
                Gamemode: {gamemode}
                Description: {description}
                Date: {date}
                Size: {size}";
            }
            catch (Exception ex)
            {
                metadataBox.Text = $"Metadata read error:\n{ex.Message}";
            }
        }

        // ------------------------------------------------
        // PROGRESS BAR
        // ------------------------------------------------

        private void OnArchiveProgressChanged(object? sender, ArchiveProgressEventArgs e)
        {
            if (progressBar == null || e.TotalFiles == 0)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                progressBar.Value = (double)e.FilesProcessed / e.TotalFiles * 100;
            });
        }

        // ------------------------------------------------
        // RELOAD
        // ------------------------------------------------

        private void ReloadClicked(object? sender, RoutedEventArgs e)
        {
            Logger.Log("Reloading worlds...");
            LoadWorlds();
        }

        // ------------------------------------------------
        // BACKUP
        // ------------------------------------------------

        private async void BackupClicked(object? sender, RoutedEventArgs e)
        {
            var world = GetSelectedWorld();

            if (world == null)
                return;

            string description = backupNameBox?.Text;

            if (string.IsNullOrWhiteSpace(description))
                description = Save.ManualSaveDescription;

            Logger.Log($"Backup started for {world.Name}");

            var save = new Save(world, description);

            save.ArchiveProgressChanged += OnArchiveProgressChanged;

            await save.ExportAsync(CancellationToken.None);

            if (progressBar != null)
                progressBar.Value = 0;

            Logger.Log($"Backup finished for {world.Name}");

            if (backupNameBox != null)
                backupNameBox.Text = "";

            LoadBackups(world);
        }

        // ------------------------------------------------
        // RESTORE
        // ------------------------------------------------

        private async void RestoreClicked(object? sender, RoutedEventArgs e)
        {
            var save = GetSelectedBackup();

            if (save == null)
            {
                Logger.Log("No backup selected.", LogSeverity.Warning);
                return;
            }

            Logger.Log($"Restoring backup {save.Description}");

            await save.RestoreAsync();

            Logger.Log("Restore completed.");
        }

        // ------------------------------------------------
        // DELETE
        // ------------------------------------------------

        private async void DeleteClicked(object? sender, RoutedEventArgs e)
        {
            var save = GetSelectedBackup();

            if (save == null)
            {
                Logger.Log("No backup selected.", LogSeverity.Warning);
                return;
            }

            Logger.Log($"Deleting backup {save.Description}");

            await save.DeleteAsync();

            Logger.Log("Backup deleted.");

            var world = GetSelectedWorld();

            if (world != null)
                LoadBackups(world);
        }

        // ------------------------------------------------
        // OPEN FOLDER
        // ------------------------------------------------

        private void OpenFolderClicked(object? sender, RoutedEventArgs e)
        {
            Logger.Log($"Opening backup folder {Save.BackupPath}");

            FileExplorer.Browse(Save.BackupPath);
        }

        // ================================================================
        // ====================== NEW FEATURES =============================
        // ================================================================

        // ------------------------------------------------
        // BACKUP SEARCH
        // ------------------------------------------------

        private void BackupSearchChanged(object? sender, RoutedEventArgs e)
        {
            if (backupSearchBox == null || backupList == null)
                return;

            string text = backupSearchBox.Text?.ToLower() ?? "";

            backupList.Items.Clear();

            foreach (var save in currentBackups)
            {
                if (save.Description.ToLower().Contains(text))
                    backupList.Items.Add(save.Description);
            }
        }

        // ------------------------------------------------
        // BACKUP RENAME
        // ------------------------------------------------

        private async void RenameClicked(object? sender, RoutedEventArgs e)
        {
            var save = GetSelectedBackup();

            if (save == null)
            {
                Logger.Log("No backup selected for rename.", LogSeverity.Warning);
                return;
            }

            if (backupNameBox == null)
                return;

            string newName = backupNameBox.Text;

            if (string.IsNullOrWhiteSpace(newName))
                return;

            await save.RenameAsync(newName);

            Logger.Log($"Backup renamed to {newName}");

            var world = GetSelectedWorld();

            if (world != null)
                LoadBackups(world);
        }

        // ------------------------------------------------
        // BACKUP SIZE
        // ------------------------------------------------

        private string GetBackupSize(Save save)
        {
            try
            {
                if (save.ArchivePath == null)
                    return "Unknown";

                long bytes = new FileInfo(save.ArchivePath).Length;

                return FormatBytes(bytes);
            }
            catch
            {
                return "Unknown";
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };

            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
