using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher_Updater
{
    public class LauncherModel
    {
        public bool success { get; set; }
        public LauncherResponseModel response { get; set; }
    }

    public class LauncherResponseModel
    {
        public string path { get; set; }
        public string hash { get; set; }
    }
}
