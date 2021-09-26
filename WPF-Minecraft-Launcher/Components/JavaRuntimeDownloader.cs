using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using WPF_Minecraft_Launcher.Models;
using CmlLib.Utils;
using System.ComponentModel;
using CmlLib.Core;

namespace WPF_Minecraft_Launcher.Components
{
    public class JavaRuntimeDownloader
    {
        public event ProgressChangedEventHandler? ProgressChanged;
        public Action<string>? DownloadCompleted;
        public string JavaRuntimeDirectoryPath = "";
        public string JavaRuntimeFilePath = "";
        public string JavaVersionCacheHashPath = "";

        public JavaRuntimeDownloader()
        {
            JavaRuntimeDirectoryPath = Path.Combine(Global.MinecraftPath, "runtime");
            JavaVersionCacheHashPath = Path.Combine(Global.ConfigPath, "javaversionhash.dat");
        }

        public void CheckJava()
        {
            var BinaryName = "java";
            if (MRule.OSName == MRule.Windows)
                BinaryName = "javaw.exe";

            CheckJava(BinaryName);
        }

        public void CheckJava(string BinaryName)
        {
            JavaRuntimeFilePath = Path.Combine(JavaRuntimeDirectoryPath, "bin", BinaryName);

            try
            {
                string GetActualJavaVersionURL = Global.GetApiAddress("javaversion/get_actual");
                ActualVersionModel? ActualVersion = null;

                using (var client = new WebClient())
                {
                    string ActualVersionJsonText = client.DownloadString(GetActualJavaVersionURL);
                    ActualVersion = JsonConvert.DeserializeObject<ActualVersionModel>(ActualVersionJsonText);
                }

                if (ActualVersion == null)
                    throw new Exception("Failed to get the latest version of java.");

                if (File.Exists(JavaVersionCacheHashPath) && File.Exists(JavaRuntimeFilePath))
                {
                    string SavedJavaVersionHash = File.ReadAllText(JavaVersionCacheHashPath);
                    if (SavedJavaVersionHash == ActualVersion.response.hash)
                    {
                        DownloadCompleted?.Invoke(JavaRuntimeFilePath);
                        return;
                    }
                }

                string DownloadVersionCachePath = Path.Combine(Global.CachePath, ActualVersion.response.tag);

                if (File.Exists(DownloadVersionCachePath))
                    File.Delete(DownloadVersionCachePath);

                if (!Directory.Exists(JavaRuntimeDirectoryPath))
                    Directory.CreateDirectory(JavaRuntimeDirectoryPath);

                DirectoryInfo RuntimeDirectoryInfo = new DirectoryInfo(JavaRuntimeDirectoryPath);

                foreach (FileInfo file in RuntimeDirectoryInfo.GetFiles())
                    file.Delete();

                foreach (DirectoryInfo directory in RuntimeDirectoryInfo.GetDirectories())
                    directory.Delete(true);

                using (var client = new WebClient())
                {
                    Global.MainWindowUI.WriteTextToLogBox(ActualVersion.response.link);

                    client.DownloadProgressChanged += DownloadVersion_DownloadProgressChanged;
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
                    {
                        var z = new SharpZip(DownloadVersionCachePath);
                        z.ProgressEvent += UnzipProgressEvent;
                        z.Unzip(JavaRuntimeDirectoryPath);

                        if (!File.Exists(JavaRuntimeFilePath))
                            throw new Exception("Failed Download");

                        if (MRule.OSName != MRule.Windows)
                            IOUtil.Chmod(JavaRuntimeFilePath, IOUtil.Chmod755);

                        File.WriteAllText(JavaVersionCacheHashPath, ActualVersion.response.hash);

                        if (File.Exists(DownloadVersionCachePath))
                            File.Delete(DownloadVersionCachePath);

                        DownloadCompleted?.Invoke(JavaRuntimeFilePath);
                    });

                    client.DownloadFileAsync(new Uri(ActualVersion.response.link), DownloadVersionCachePath);
                }
            }
            catch(Exception ex)
            {
                Global.LauncherLogger.Write(ex.ToString(), "ERROR");
                return;
            }
        }

        private void UnzipProgressEvent(object sender, int e)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(e, null));
        }

        private void DownloadVersion_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }
    }
}
