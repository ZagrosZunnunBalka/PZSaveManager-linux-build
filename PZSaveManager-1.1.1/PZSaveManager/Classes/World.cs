using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml.Linq;
using PZSaveManager.Properties;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using IOPath = System.IO.Path;

namespace PZSaveManager.Classes
{
    public class World
    {
        public static readonly string DefaultDuserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static string BaseDirectory =>
        Directory.Exists(Settings.Default.DuserPath)
        ? IOPath.Combine(Settings.Default.DuserPath, "Zomboid")
        : IOPath.Combine(DefaultDuserPath, "Zomboid");

        public static string WorldDirectory => IOPath.Combine(BaseDirectory, "Saves");

        private const string LockedFileName = "players.db";
        private const string BackupSuffix = "__old";
        private const string SaveNoDescription = "No description";

        private static readonly object isActiveLock = new();

        public const string ThumbName = "thumb.png";
        public const int ThumbWidth = 256;

        public string Name { get; }
        public string Path { get; }
        public string Gamemode { get; }

        public string GamemodePath { get; }
        public string BackupPath { get; }

        public DateTime LastActive => Directory.GetLastWriteTime(Path);

        public bool IsActive
        {
            get
            {
                lock (isActiveLock)
                {
                    string lockedFilePath = IOPath.Combine(Path, LockedFileName);

                    if (!File.Exists(lockedFilePath))
                        return false;

                    try
                    {
                        using (File.Open(lockedFilePath, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                        return false;
                    }
                    catch (IOException)
                    {
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public World(string name, string path, string gamemode)
        {
            Name = name;
            Path = path;
            Gamemode = gamemode;

            GamemodePath = IOPath.Combine(WorldDirectory, Gamemode);
            BackupPath = IOPath.Combine(GamemodePath, Name + BackupSuffix);
        }

        public IEnumerable<Save> GetSaves()
        => GetAllSaves().Where(s =>
        s.AssociatedWorld is not null &&
        Name == s.AssociatedWorld.Name &&
        Gamemode == s.AssociatedWorld.Gamemode);

        private static IEnumerable<Save> GetOrphanedSaves()
        => GetAllSaves().Where(s => s.AssociatedWorld is null);

        private static List<Save> GetAllSaves()
        {
            var saves = new ConcurrentBag<Save>();

            if (!Directory.Exists(Save.BackupPath))
                return saves.ToList();

            var allWorlds = GetAllWorlds().ToList();

            Parallel.ForEach(Directory.EnumerateDirectories(Save.BackupPath), folderPath =>
            {
                string? archivePath = GetArchivePath();

                if (archivePath is null)
                    return;

                string metadataPath = IOPath.Combine(folderPath, Save.MetadataFileName);

                string? worldName = null;
                string? worldGamemode = null;
                string? description = null;
                DateTime? date = null;

                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var xml = XElement.Load(metadataPath);

                        worldName = xml.Element(XmlElementName.Metadata.WorldName)?.Value;
                        worldGamemode = xml.Element(XmlElementName.Metadata.WorldGamemode)?.Value;
                        description = xml.Element(XmlElementName.Metadata.Description)?.Value;

                        if (DateTime.TryParse(xml.Element(XmlElementName.Metadata.Date)?.Value, out var parsedDate))
                            date = parsedDate;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Could not process the metadata at {metadataPath}", ex);
                    }
                }

                if (worldName is null || worldGamemode is null)
                {
                    var worldData = Save.GetWorldDataFromArchive(archivePath);

                    if (worldData is null)
                    {
                        Logger.Log($"The save archive at {archivePath} appears to be corrupt. Skipping.", LogSeverity.Warning);
                        return;
                    }

                    worldName = worldData.Value.Name;
                    worldGamemode = worldData.Value.Gamemode;
                }

                string thumbPath = IOPath.Combine(folderPath, Save.ThumbFileName);
                var thumb = new MemoryStream();

                try
                {
                    if (File.Exists(thumbPath))
                    {
                        using var thumbFile = File.OpenRead(thumbPath);
                        thumbFile.CopyTo(thumb);
                    }
                    else
                    {
                        using var archive = ArchiveFactory.Open(archivePath);
                        using var ts = archive.Entries.FirstOrDefault(e => e.Key == $"{worldGamemode}/{worldName}/{ThumbName}")?.OpenEntryStream();
                        ts?.CopyTo(thumb);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Could not load the thumb for world {worldName} ({worldGamemode})", ex);
                }

                thumb.Position = 0;

                date ??= File.GetLastWriteTime(archivePath);

                saves.Add(new Save(GetAssociatedWorld(), description ?? SaveNoDescription, archivePath, date.Value, thumb));

                string? GetArchivePath()
                {
                    string zipPath = IOPath.Combine(folderPath, Save.ArchiveFileName + ".zip");
                    string tarPath = IOPath.Combine(folderPath, Save.ArchiveFileName + ".tar");

                    if (File.Exists(zipPath) && ZipArchive.IsZipFile(zipPath))
                        return zipPath;

                    if (File.Exists(tarPath) && TarArchive.IsTarFile(tarPath))
                        return tarPath;

                    return null;
                }

                World? GetAssociatedWorld()
                {
                    if (string.IsNullOrWhiteSpace(worldName))
                        return null;

                    try
                    {
                        return !string.IsNullOrWhiteSpace(worldGamemode)
                        ? allWorlds.FirstOrDefault(w => w.Name == worldName && w.Gamemode == worldGamemode)
                        : allWorlds.FirstOrDefault(w => w.Name == worldName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Could not find any world called {worldName} ({worldGamemode})", ex);
                        return null;
                    }
                }
            });

            return saves.ToList();
        }

        public static IEnumerable<World> GetAllWorlds()
        {
            if (!Directory.Exists(BaseDirectory))
                throw new DirectoryNotFoundException("Could not locate the save folder. Project Zomboid is likely not installed.");

            if (!Directory.Exists(WorldDirectory))
                throw new DirectoryNotFoundException("Project Zomboid is installed, but the save folder could not be located.");

            foreach (string gamemodePath in Directory.EnumerateDirectories(WorldDirectory))
            {
                foreach (string worldPath in Directory.EnumerateDirectories(gamemodePath))
                {
                    string tarPath = IOPath.Combine(worldPath, $"{Save.ArchiveFileName}.tar");
                    string zipPath = IOPath.Combine(worldPath, $"{Save.ArchiveFileName}.zip");

                    if (!File.Exists(tarPath) && !File.Exists(zipPath))
                        yield return new(IOPath.GetFileName(worldPath), worldPath, IOPath.GetFileName(gamemodePath));
                }
            }
        }
    }
}
