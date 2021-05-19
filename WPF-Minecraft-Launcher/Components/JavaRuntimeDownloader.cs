using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WPF_Minecraft_Launcher.Models;
using CmlLib.Utils;
using System.ComponentModel;
using CmlLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;

namespace WPF_Minecraft_Launcher.Components
{
    public class JavaRuntimeDownloader
    {
        public event ProgressChangedEventHandler ProgressChanged;
        public Action<string> DownloadCompleted;
        public string runtime_directory = "";
        public string javapath = "";
        public string hash_cache_path = "";

        public JavaRuntimeDownloader()
        {
            runtime_directory = Path.Combine(Global.MinecraftPath, "runtime");
            hash_cache_path = Path.Combine(Global.ConfigPath, "javaversionhash.dat");
        }

        public void CheckJava()
        {
            var binaryName = "java";
            if (MRule.OSName == MRule.Windows)
                binaryName = "javaw.exe";

            CheckJava(binaryName);
        }

        public void CheckJava(string binaryName)
        {
            javapath = Path.Combine(runtime_directory, "bin", binaryName);

            try
            {
                string get_version_url = Global.GetApiAddress("javaversion/get_actual");
                ActualVersionModel actual_version = null;

                using (var wc = new WebClient())
                {
                    string json = wc.DownloadString(get_version_url);
                    actual_version = JsonConvert.DeserializeObject<ActualVersionModel>(json);
                }

                if (actual_version == null)
                    throw new Exception("Failed to get the latest version of java.");

                if (File.Exists(hash_cache_path) && File.Exists(javapath))
                {
                    string old_hash = File.ReadAllText(hash_cache_path);
                    if (old_hash == actual_version.response.hash)
                    {
                        DownloadCompleted.Invoke(javapath);
                        return;
                    }
                }

                string saved_path = Path.Combine(Global.CachePath, actual_version.response.tag);

                if (File.Exists(saved_path))
                    File.Delete(saved_path);

                if (!Directory.Exists(runtime_directory))
                    Directory.CreateDirectory(runtime_directory);

                DirectoryInfo di = new DirectoryInfo(runtime_directory);

                foreach (FileInfo file in di.GetFiles())
                    file.Delete();

                foreach (DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);

                using (var client = new WebClient())
                {
                    Global.mainWindow.AddLineToLog(actual_version.response.link);

                    client.DownloadProgressChanged += DownloadVersion_DownloadProgressChanged;
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
                    {
                        var z = new SharpZip(saved_path);
                        z.ProgressEvent += Z_ProgressEvent;
                        z.Unzip(runtime_directory);

                        if (!File.Exists(javapath))
                            throw new Exception("Failed Download");

                        if (MRule.OSName != MRule.Windows)
                            IOUtil.Chmod(javapath, IOUtil.Chmod755);

                        File.WriteAllText(hash_cache_path, actual_version.response.hash);

                        if (File.Exists(saved_path))
                            File.Delete(saved_path);

                        DownloadCompleted.Invoke(javapath);
                    });

                    client.DownloadFileAsync(new Uri(actual_version.response.link), saved_path);
                }
            }
            catch(Exception ex)
            {
                Global.LauncherLogger.Write(ex.ToString(), "ERROR");
                return;
            }
        }

        private void Z_ProgressEvent(object sender, int e)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(e, null));
        }

        private void DownloadVersion_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
