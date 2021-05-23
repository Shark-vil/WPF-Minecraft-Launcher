using Newtonsoft.Json;
using System;
using System.IO;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class Global
    {
        internal static LauncherConfigModel LauncherConfig = new LauncherConfigModel();
        internal static string ApplicationPath = AppContext.BaseDirectory;
        internal static string MinecraftPath = Path.Combine(ApplicationPath, "minecraft");
        internal static string ConfigPath = Path.Combine(ApplicationPath, "launcher");
        internal static string CachePath = Path.Combine(ConfigPath, "cache");

        internal static Logger? LauncherLogger;
        internal static Logger? GameLogger;
        internal static Logger? GameErrorsLogger;
        internal static MainWindow? MainWindowUI;

        internal static void LauncherConfigInit()
        {
            if (!Directory.Exists(MinecraftPath))
                Directory.CreateDirectory(MinecraftPath);

            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);

            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);

            LauncherConfig.LogFileName = "pipbuck-launcher.log";
            LauncherConfig.GameLogFileName = "pipbuck-game-session.log";
            LauncherConfig.GameErrorsLogFileName = "pipbuck-game-errors-session.log";
            LauncherConfig.SiteAddress = "https://mc.pipbuck.ru";
            LauncherConfig.ServerIP = "95.216.122.173";
            LauncherConfig.ServerPort = 25565;
            LauncherConfig.MaxRAM = 4096;
            LauncherConfig.AuthserverAddress = LauncherConfig.SiteAddress + "/api/authserver/";

            string ConfigFilePath = Path.Combine(ConfigPath, "config.json");
            if (File.Exists(ConfigFilePath))
            {
                string ConfigJsonText = File.ReadAllText(ConfigFilePath);
                LauncherConfig = JsonConvert.DeserializeObject<LauncherConfigModel>(ConfigJsonText);
            }
            else
            {
                string config_json = JsonConvert.SerializeObject(LauncherConfig, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, config_json);
            }

            LauncherLogger = new Logger(LauncherConfig.LogFileName);
            GameLogger = new Logger(LauncherConfig.GameLogFileName);
            GameErrorsLogger = new Logger(LauncherConfig.GameErrorsLogFileName);
        }

        internal static string GetAuthserverApiAddress(string ServiceAddress)
        {
            return LauncherConfig.AuthserverAddress + ServiceAddress;
        }

        internal static string GetApiAddress(string ServiceAddress)
        {
            return LauncherConfig.SiteAddress + "/api/" + ServiceAddress;
        }
    }
}
