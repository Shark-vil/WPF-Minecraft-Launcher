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
        public static LauncherConfigModel LauncherConfig = new LauncherConfigModel();
        public static void LauncherConfigInit()
        {
            LauncherConfig.MinecraftVersion = "1.16.5";
            LauncherConfig.MinecraftPath = @"F:\MinecraftCustom\client";
            LauncherConfig.LogPath = "pipbuck-launcher.log";
        }
    }
}
