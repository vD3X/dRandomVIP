using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static dRandomVIP.dRandomVIP;

namespace dRandomVIP
{
    public static class Config
    {
        private static readonly string configPath = Path.Combine(Instance.ModuleDirectory, "Config.json");
        public static ConfigModel config;
        private static FileSystemWatcher fileWatcher;

        public static ConfigModel LoadedConfig => config;

        public static void Initialize()
        {
            config = LoadConfig();
            SetupFileWatcher();
        }

        private static ConfigModel LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                Instance.Logger.LogInformation("Plik konfiguracyjny nie istnieje. Tworzenie nowego pliku konfiguracyjnego...");
                var defaultConfig = new ConfigModel();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<ConfigModel>(json) ?? new ConfigModel();
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError($"Błąd podczas wczytywania pliku konfiguracyjnego.");
                return new ConfigModel();
            }
        }

        public static void SaveConfig(ConfigModel config)
        {
            try
            {
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError($"Błąd podczas zapisywania pliku konfiguracyjnego: {ex.Message}");
            }
        }

        private static void SetupFileWatcher()
        {
            fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(configPath))
            {
                Filter = Path.GetFileName(configPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            fileWatcher.Changed += (sender, e) => config = LoadConfig();
            fileWatcher.EnableRaisingEvents = true;
        }

        public class ConfigModel
        {
            public Settings Settings { get; set; } = new Settings();
            public Messages Messages { get; set; } = new Messages();
        }

        public class Settings
        {
            public string Prefix { get; set; } = "{DARKRED}► {GREEN}[{DARKRED} LOSOWY VIP {GREEN}] {GREEN}✔ {LIME}";
            public string Flag { get; set; } = "@cszjarani/vip";
            public int AfterXRounds { get; set; } = 4;
            public int MinPlayers { get; set; } = 3;
        }

        public class Messages
        {
            public string NotEnoughPlayers { get; set; } = "Jest zbyt mało graczy aby wylosować {PURPLE}VIPA{LIME}.";
            public string AllPlayersAreVIP { get; set; } = "Na serwerze są same VIPY.";
            public string DrawingStarted { get; set; } = "Trwa losowanie {PURPLE}VIPA{LIME}...";
            public string DrawingSeparator { get; set; } = "----";
            public string VIPSelected { get; set; } = "Losowego {PURPLE}VIPA {LIME}otrzymuje {DARKRED}{PLAYERNAME}{LIME}.";
        }
    }
}