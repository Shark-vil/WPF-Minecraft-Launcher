using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Components
{
    public class Logger
    {
        internal static void Init()
        {
            if (File.Exists(Global.LauncherConfig.LogPath))
                File.Delete(Global.LauncherConfig.LogPath);
        }

        internal static void Write(string text, string logtype = "LOG")
        {
            if (text == null)
                return;

            logtype = logtype.ToUpper();

            string dateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            string logFilePath = Path.Combine(Global.LauncherConfig.MinecraftPath, Global.LauncherConfig.LogPath);

            if (File.Exists(logFilePath))
                File.AppendAllText(logFilePath, string.Format("[{0}][{1}] {2}", dateTime, logtype, text + "\n"));
            else
                File.WriteAllText(logFilePath, string.Format("[{0}][{1}] {2}", dateTime, logtype, text));
        }
    }
}
