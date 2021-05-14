using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    [Serializable]
    public class LauncherConfigModel
    {
        public string MinecraftPath { get; set; }
        public string MinecraftVersion { get; set; }
        public string LogFileName { get; set; }
        public string GameLogFileName { get; set; }
        public string GameErrorsLogFileName { get; set; }
        public string SiteAddress { get; set; }
    }
}
