using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher_Updater
{
    class Program
    {
        private static string ApplicationDirectoryPath = AppContext.BaseDirectory;
        private static string LogFilePath = Path.Combine(ApplicationDirectoryPath, "laucnherupdate.log");

        static async Task Main(string[] args)
        {
            string ApplicationParentDirectoryPath = new FileInfo(ApplicationDirectoryPath).Directory.Parent.FullName;
            string ConfigirectoryPath = Path.Combine(ApplicationParentDirectoryPath, "launcher");

            _ = Task.Delay(1000);

            if (!Directory.Exists(ConfigirectoryPath))
                Directory.CreateDirectory(ConfigirectoryPath);

            if (File.Exists(LogFilePath))
                File.Delete(LogFilePath);

            string ConfigPath = Path.Combine(ConfigirectoryPath, "config.json");
            if (!File.Exists(ConfigPath))
            {
                print("File 'config.json' not found");
                return;
            }

            try
            {
                string ConfigData = File.ReadAllText(ConfigPath);
                string SiteAddress = JObject.Parse(ConfigData)["SiteAddress"].ToString();
                string VersionFilePath = Path.Combine(ConfigirectoryPath, "version.dat");
                LauncherModel LauncherObject;

                print("Requesting version information");

                using (var client = new WebClient())
                {
                    string JsonData = client.DownloadString(SiteAddress + "/api/launcherversion/get_actual");
                    LauncherObject = JsonConvert.DeserializeObject<LauncherModel>(JsonData);

                    if (File.Exists(VersionFilePath))
                    {
                        string CurrentVersion = File.ReadAllText(VersionFilePath);
                        if (CurrentVersion == LauncherObject.response.tag)
                        {
                            print("The version is the same, no update is required");
                            return;
                        }
                        else
                            print("The version does not match, an update is required.");
                    }
                    else
                        print("The hash file does not exist");
                }

                if (LauncherObject != null)
                {
                    print("Preparing to download the update archive");

                    using (var client = new WebClient())
                    {
                        string FileName = "update.zip";
                        string FilePath = Path.Combine(ApplicationDirectoryPath, FileName);
                        var VisualProgressBar = new ProgressBar();

                        client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                        {
                            int percent = e.ProgressPercentage;
                            VisualProgressBar.Report((double) percent / 100);
                        };

                        if (File.Exists(FilePath))
                            File.Delete(FilePath);

                        await client.DownloadFileTaskAsync(new Uri(LauncherObject.response.link), FilePath);

                        VisualProgressBar.Dispose();

                        print("Archive download completed");

                        if (File.Exists(FilePath))
                        {
                            print("Unzip the archive");

                            ZipFile.ExtractToDirectory(FilePath, ApplicationParentDirectoryPath, true);

                            print("Update download completed");

                            File.Delete(FilePath);
                            File.WriteAllText(VersionFilePath, LauncherObject.response.tag);
                        }
                        else
                            print("The downloaded archive file was not found");
                    }
                }
                else
                    print("Failed to convert object with launcher version");
            }
            catch(Exception ex)
            {
                print($"An exception was thrown during the update:\n{ex}");
            }
        }

        private static void print(object text)
        {
            string NormalizeText = string.Format("[{0}] {1}", DateTime.Now, Convert.ToString(text));
            Console.WriteLine(NormalizeText);
            File.AppendAllText(LogFilePath, NormalizeText + "\n");
        }
    }
}
