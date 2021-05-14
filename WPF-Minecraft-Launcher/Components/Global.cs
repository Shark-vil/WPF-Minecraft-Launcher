using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class Global
    {
        internal static LauncherConfigModel LauncherConfig = new LauncherConfigModel();
        internal static Logger LauncherLogger;
        internal static Logger GameLogger;
        internal static Logger GameErrorsLogger;
        internal static MainWindow mainWindow;

        internal static void LauncherConfigInit()
        {
            LauncherConfig.MinecraftVersion = "1.16.5";
            LauncherConfig.MinecraftPath = @"F:\MinecraftCustom\client";
            LauncherConfig.LogFileName = "pipbuck-launcher.log";
            LauncherConfig.GameLogFileName = "pipbuck-game-session.log";
            LauncherConfig.GameErrorsLogFileName = "pipbuck-game-errors-session.log";
            LauncherConfig.SiteAddress = "http://wpf-minecraft-launcher-web.localhost/api/authserver/";

            LauncherLogger = new Logger(LauncherConfig.LogFileName);
            GameLogger = new Logger(LauncherConfig.GameLogFileName);
            GameErrorsLogger = new Logger(LauncherConfig.GameErrorsLogFileName);
        }

        internal static string GetApiAddress(string ServiceAddress)
        {
            return LauncherConfig.SiteAddress + ServiceAddress;
        }
    }
}
