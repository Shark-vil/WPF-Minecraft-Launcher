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
        private string filePath;

        public Logger(string fileName)
        {
            string directoryLogPath = Path.Combine(Global.ConfigPath, "logs");

            if (!Directory.Exists(directoryLogPath))
                Directory.CreateDirectory(directoryLogPath);

            filePath = Path.Combine(directoryLogPath, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        internal void Write(string text, string logtype = "LOG")
        {
            if (text == null)
                return;

            logtype = logtype.ToUpper();
            string dateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

            try
            {
                using (var f = new StreamWriter(filePath, true))
                {
                    f.WriteLine(string.Format("[{0}][{1}] {2}", dateTime, logtype, text));
                    f.Flush();
                    f.Close();
                }
            }
            catch(IOException ex) { }
        }
    }
}
