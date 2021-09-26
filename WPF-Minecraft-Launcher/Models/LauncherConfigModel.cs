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
        public string LogFileName { get; set; }
        public string GameLogFileName { get; set; }
        public string GameErrorsLogFileName { get; set; }
        public string AuthserverAddress { get; set; }
        public string SiteAddress { get; set; }
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public int MaxRAM { get; set; }
    }
}
