using CmlLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class AddonsDownloader
    {
        public event ProgressChangedEventHandler ProgressChanged;
        public Action DownloadCompleted;

        private string get_addons_address;
        private string get_removed_addons_address;
        private string cache_path;
        private string mods_path;
        private string[] find_mods;
        private Dictionary<string, string> find_mods_and_hashes;
        private List<string> cache_hashes = new List<string>();

        public AddonsDownloader()
        {
            get_addons_address = Global.GetApiAddress("addon/get");
            get_removed_addons_address = Global.GetApiAddress("addon/get_del");
            cache_path = Path.Combine(Global.ConfigPath, "addons.json");
            mods_path = Path.Combine(Global.MinecraftPath, "mods");

            if (!Directory.Exists(mods_path))
                Directory.CreateDirectory(mods_path);

            find_mods = Directory.GetFiles(mods_path, "*.jar");
            find_mods_and_hashes = new Dictionary<string, string>();

            if (File.Exists(cache_path))
            {
                using (var reader = new StreamReader(cache_path))
                {
                    string text = reader.ReadToEnd();
                    List<string> read_cache_hashes = JsonConvert.DeserializeObject<List<string>>(text);

                    if (read_cache_hashes != null)
                        cache_hashes = read_cache_hashes;

                    reader.Close();
                }
            }
        }

        public async void Download()
        {
            try
            {
                foreach(string fileName in find_mods)
                {
                    string full_path = Path.Combine(mods_path, fileName);
                    string hash = Helpers.CalculateFileMD5(full_path);
                    find_mods_and_hashes.Add(hash, full_path);
                }

                Global.mainWindow.AddLineToLog($"Start addons downloading");

                AddonModel addons;
                DelAddonModel removed_addon;
                string json_addons = "";
                string json_removed_addons = "";

                var requestGetAddons = new LWebRequest();
                requestGetAddons.SetAddress(get_addons_address);
                json_addons = requestGetAddons.Send(HttpMethod.Get).htmlTextMessage;

                var requestGetRemovedAddons = new LWebRequest();
                requestGetRemovedAddons.SetAddress(get_removed_addons_address);
                json_removed_addons = requestGetRemovedAddons.Send(HttpMethod.Get).htmlTextMessage;

                addons = JsonConvert.DeserializeObject<AddonModel>(json_addons);
                removed_addon = JsonConvert.DeserializeObject<DelAddonModel>(json_removed_addons);

                if (removed_addon != null)
                {
                    List<DelAddonResponseModel> response = removed_addon.response;
                    int total_ptrogress = response.Count;
                    int current_progress = 0;

                    foreach (DelAddonResponseModel addon in response)
                    {
                        string hash = addon.hash;
                        if (cache_hashes.Exists(x => x == hash))
                        {
                            string path = find_mods_and_hashes.FirstOrDefault(x => x.Key == hash).Value;
                            if (path != null && path.Length != 0)
                            {
                                File.Delete(path);
                                cache_hashes.RemoveAll(x => x == hash);

                                Global.mainWindow.AddLineToLog($"Remove addon - {addon.name}");
                            }
                        }

                        current_progress++;

                        int result = (current_progress / total_ptrogress) * 100;
                        DownloadVersion_DownloadProgressChanged(null, new ProgressChangedEventArgs(result, null));
                    }
                }

                if (addons == null || addons.response.Count == 0)
                    Complete();
                else
                {
                    List<AddonResponseModel> response = addons.response;
                    int total_ptrogress = response.Count;
                    int current_progress = 0;

                    string adons_cache_path = Path.Combine(Global.CachePath, "addons");
                    if (!Directory.Exists(adons_cache_path))
                        Directory.CreateDirectory(adons_cache_path);

                    foreach (AddonResponseModel addon in response)
                    {
                        string hash = addon.hash;
                        string zipFilePath = Path.Combine(adons_cache_path, addon.name.Replace(".jar", ".zip"));
                        string findAddon = find_mods_and_hashes.FirstOrDefault(x => x.Key == hash).Value;
                        bool isExist = cache_hashes.Exists(x => x == hash);

                        if (isExist && findAddon != null)
                            RecalculateProgress(ref current_progress, total_ptrogress);
                        else
                        {
                            using (var client = new WebClient())
                            {
                                client.DownloadProgressChanged += DownloadVersion_DownloadProgressChanged;
                                client.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
                                {
                                    var z = new SharpZip(zipFilePath);
                                    z.ProgressEvent += Z_ProgressEvent;
                                    z.Unzip(mods_path);

                                    if (File.Exists(zipFilePath))
                                        File.Delete(zipFilePath);

                                    if (!cache_hashes.Exists(x => x == hash))
                                        cache_hashes.Add(hash);

                                    Global.mainWindow.AddLineToLog($"Add new addon - {addon.name}");
                                    RecalculateProgress(ref current_progress, total_ptrogress);
                                });

                                await client.DownloadFileTaskAsync(new Uri(addon.link), zipFilePath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Global.LauncherLogger.Write(ex.ToString(), "ERROR");
                return;
            }
        }

        private void RecalculateProgress(ref int current_progress, int total_ptrogress)
        {
            current_progress++;
            int result = (current_progress / total_ptrogress) * 100;
            DownloadVersion_DownloadProgressChanged(null, new ProgressChangedEventArgs(result, null));

            if (current_progress == total_ptrogress)
                Complete();
        }

        private void Complete()
        {
            try
            {
                string text = JsonConvert.SerializeObject(cache_hashes);
                using (var writer = new StreamWriter(cache_path))
                {
                    writer.Write(text);
                    writer.Close();
                }
            }
            catch { }

            Global.mainWindow.AddLineToLog($"Downloading addons completed");
            DownloadCompleted.Invoke();
        }
        private void Z_ProgressEvent(object sender, int e)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(e, null));
        }

        private void DownloadVersion_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        private void DownloadVersion_DownloadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
