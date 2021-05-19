using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class MinecraftVersionDownloader
    {
        public event ProgressChangedEventHandler ProgressChanged;
        public Action<string> DownloadCompleted;
        public string version_directory = "";
        public string hash_cache_path = "";

        public MinecraftVersionDownloader()
        {
            version_directory = Path.Combine(Global.MinecraftPath, "versions");
            hash_cache_path = Path.Combine(Global.ConfigPath, "minecraftversionhash.dat");
        }

        public void Download()
        {
            try
            {
                string get_version_url = Global.GetApiAddress("minecraftversion/get_actual");
                MinecraftVersionModel minecraft_version = null;

                using (var wc = new WebClient())
                {
                    string json = wc.DownloadString(get_version_url);
                    minecraft_version = JsonConvert.DeserializeObject<MinecraftVersionModel>(json);
                }

                if (minecraft_version == null)
                    throw new Exception("Failed to get the latest version of minecraft.");

                string tag = minecraft_version.response.tag;
                string json_version_path = Path.Combine(version_directory, tag, $"{tag}.json");

                if (File.Exists(hash_cache_path) && File.Exists(json_version_path))
                {
                    string old_hash = File.ReadAllText(hash_cache_path);
                    if (old_hash == minecraft_version.response.hash)
                    {
                        DownloadCompleted.Invoke(minecraft_version.response.tag);
                        return;
                    }
                }

                string saved_directory_path = Path.Combine(version_directory, tag);

                if (!Directory.Exists(saved_directory_path))
                    Directory.CreateDirectory(saved_directory_path);

                string saved_path = Path.Combine(saved_directory_path, $"{tag}.json");

                if (File.Exists(saved_path))
                    File.Delete(saved_path);

                using (var client = new WebClient())
                {
                    Global.mainWindow.AddLineToLog(minecraft_version.response.link);

                    client.DownloadProgressChanged += DownloadVersion_DownloadProgressChanged;
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
                    {

                        if (!File.Exists(saved_path))
                            throw new Exception("Failed Download");

                        File.WriteAllText(hash_cache_path, minecraft_version.response.hash);

                        DownloadCompleted.Invoke(minecraft_version.response.tag);
                    });

                    client.DownloadFileAsync(new Uri(minecraft_version.response.link), saved_path);
                }
            }
            catch (Exception ex)
            {
                Global.LauncherLogger.Write(ex.ToString(), "ERROR");
                return;
            }
        }

        private void DownloadVersion_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
