using CmlLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class AddonsDownloader
    {
        public event ProgressChangedEventHandler? ProgressChanged;
        public Action? DownloadCompleted;

        private string GetAddonsURL;
        private string AddonsCachePath;
        private string MinecraftModsPath;
        private string[] FindJarMods;
        private Dictionary<string, string> ModsAndHashesExists;
        private List<string> AddonsCacheHashesList = new List<string>();

        public AddonsDownloader()
        {
            GetAddonsURL = Global.GetApiAddress("addon/get");
            AddonsCachePath = Path.Combine(Global.ConfigPath, "addons.json");
            MinecraftModsPath = Path.Combine(Global.MinecraftPath, "mods");

            if (!Directory.Exists(MinecraftModsPath))
                Directory.CreateDirectory(MinecraftModsPath);

            FindJarMods = Directory.GetFiles(MinecraftModsPath, "*.jar");
            ModsAndHashesExists = new Dictionary<string, string>();

            if (File.Exists(AddonsCachePath))
            {
                using (var reader = new StreamReader(AddonsCachePath))
                {
                    string AddonsHashesJsonText = reader.ReadToEnd();
                    var HashesList = JsonConvert.DeserializeObject<List<string>>(AddonsHashesJsonText);

                    if (HashesList != null)
                        AddonsCacheHashesList = HashesList;

                    reader.Close();
                }
            }
        }

        public async void Download()
        {
            try
            {
                foreach(string FileName in FindJarMods)
                {
                    string FullFilePath = Path.Combine(MinecraftModsPath, FileName);
                    string FileHash = Helpers.CalculateFileMD5(FullFilePath);
                    ModsAndHashesExists.Add(FileHash, FullFilePath);
                }

                Global.MainWindowUI.WriteTextToLogBox($"Start addons downloading");

                var GetAddonsReqeust = new LWebRequest();
                GetAddonsReqeust.SetAddress(GetAddonsURL);

                string JsonAddonsResponse = GetAddonsReqeust.Send(HttpMethod.Get).ResponseText;
                var AddonsResponse = JsonConvert.DeserializeObject<AddonModel>(JsonAddonsResponse);

                if (AddonsResponse == null || AddonsResponse.response.Count == 0)
                {
                    foreach(string FileHash in AddonsCacheHashesList)
                    {
                        string FilePath = ModsAndHashesExists.FirstOrDefault(x => x.Key == FileHash).Value;
                        if (FilePath != null)
                        {
                            if (File.Exists(FilePath))
                                File.Delete(FilePath);

                            AddonsCacheHashesList.RemoveAll(x => x == FileHash);
                            Global.MainWindowUI.WriteTextToLogBox($"Remove addon - {Path.GetFileName(FilePath)}");
                        }
                    }

                    Complete();
                }
                else
                {
                    List<AddonResponseModel> AddonsList = AddonsResponse.response;
                    int TotalProgress, CurrentProgress = 0;

                    string DownloadAddonsCachePath = Path.Combine(Global.CachePath, "addons");
                    if (!Directory.Exists(DownloadAddonsCachePath))
                        Directory.CreateDirectory(DownloadAddonsCachePath);

                    Dictionary<string, string> AddonsToRemoveList = new Dictionary<string, string>();
                    TotalProgress = ModsAndHashesExists.Count;

                    foreach (KeyValuePair<string, string> entry in ModsAndHashesExists)
                    {
                        string path = entry.Value;
                        string hash = entry.Key;

                        if (!AddonsList.Exists(x => x.hash == hash))
                            AddonsToRemoveList.Add(hash, path);

                        RecalculateProgress(ref CurrentProgress, TotalProgress);
                    }

                    CurrentProgress = 0;
                    TotalProgress = AddonsToRemoveList.Count;

                    foreach (KeyValuePair<string, string> entry in AddonsToRemoveList)
                    {
                        string path = entry.Value;
                        string hash = entry.Key;

                        if (File.Exists(path))
                            File.Delete(path);

                        AddonsCacheHashesList.RemoveAll(x => x == hash);
                        Global.MainWindowUI.WriteTextToLogBox($"Remove addon - {Path.GetFileName(path)}");

                        RecalculateProgress(ref CurrentProgress, TotalProgress);
                    }

                    CurrentProgress = 0;
                    TotalProgress = AddonsList.Count;

                    foreach (AddonResponseModel Addon in AddonsList)
                    {
                        string FileHash = Addon.hash;
                        string DownloadZipFilePath = Path.Combine(DownloadAddonsCachePath, Addon.name.Replace(".jar", ".zip"));
                        string FoundExistingAddon = ModsAndHashesExists.FirstOrDefault(x => x.Key == FileHash).Value;
                        bool IsExist = AddonsCacheHashesList.Exists(x => x == FileHash);

                        if (IsExist && FoundExistingAddon != null)
                        {
                            ChangeAddonState(FoundExistingAddon, Addon.enabled);
                            RecalculateProgress(ref CurrentProgress, TotalProgress);
                        }
                        else
                        {
                            using (var client = new WebClient())
                            {
                                client.DownloadProgressChanged += DownloadVersion_DownloadProgressChanged;
                                client.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
                                {
                                    var z = new SharpZip(DownloadZipFilePath);
                                    z.ProgressEvent += UnzipProgressEvent;
                                    z.Unzip(MinecraftModsPath);

                                    if (File.Exists(DownloadZipFilePath))
                                        File.Delete(DownloadZipFilePath);

                                    if (!AddonsCacheHashesList.Exists(x => x == FileHash))
                                        AddonsCacheHashesList.Add(FileHash);

                                    string DownloadedAddonFilePath = Path.Combine(MinecraftModsPath, Addon.name);
                                    ChangeAddonState(DownloadedAddonFilePath, Addon.enabled);

                                    Global.MainWindowUI.WriteTextToLogBox($"Add new addon - {Addon.name}");
                                    RecalculateProgress(ref CurrentProgress, TotalProgress);
                                });

                                await client.DownloadFileTaskAsync(new Uri(Addon.link), DownloadZipFilePath);
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

        private void ChangeAddonState(string FilePath, bool Enabled)
        {
            if (File.Exists(FilePath))
            {
                if (Enabled && Path.GetExtension(FilePath) == ".disable")
                    Path.ChangeExtension(FilePath, ".jar");
                else if (!Enabled && Path.GetExtension(FilePath) == ".jar")
                    Path.ChangeExtension(FilePath, ".disable");
            }
        }

        private void RecalculateProgress(ref int CurrentProgress, int TotalProgress)
        {
            CurrentProgress++;

            int result = (CurrentProgress / TotalProgress) * 100;
            DownloadVersion_DownloadProgressChanged(this, new ProgressChangedEventArgs(result, null));

            if (CurrentProgress == TotalProgress)
                Complete();
        }

        private void Complete()
        {
            try
            {
                string AddonsHashesJsonText = JsonConvert.SerializeObject(AddonsCacheHashesList);
                using (var writer = new StreamWriter(AddonsCachePath))
                {
                    writer.Write(AddonsHashesJsonText);
                    writer.Close();
                }
            }
            catch { }

            Global.MainWindowUI.WriteTextToLogBox($"Downloading addons completed");
            DownloadCompleted?.Invoke();
        }
        private void UnzipProgressEvent(object sender, int e)
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
