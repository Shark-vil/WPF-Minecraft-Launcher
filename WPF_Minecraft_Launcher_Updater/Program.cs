using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace WPF_Minecraft_Launcher_Updater
{
    class Program
    {
        private static string ApplicationDirectoryPath = AppContext.BaseDirectory;
        private static string ConfigirectoryPath = Path.Combine(ApplicationDirectoryPath, "launcher");
        private static string LogFilePath = Path.Combine(ApplicationDirectoryPath, "laucnherupdate.log");

        static void Main(string[] args)
        {
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
                string HashPath = Path.Combine(ConfigirectoryPath, "launcherhash.dat");
                LauncherModel LauncherObject;

                print("Requesting version information");

                using (var client = new WebClient())
                {
                    string JsonData = client.DownloadString(SiteAddress);
                    LauncherObject = JsonConvert.DeserializeObject<LauncherModel>(JsonData);

                    if (File.Exists(HashPath))
                    {
                        string CurrentHash = File.ReadAllText(HashPath);
                        if (CurrentHash == LauncherObject.response.hash)
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

                        print("Loading the archive. Please wait.");

                        client.DownloadFile(LauncherObject.response.path, FilePath);

                        print("Archive download completed");

                        if (File.Exists(FilePath))
                        {
                            print("Unzip the archive");

                            ZipFile.ExtractToDirectory(FilePath, ApplicationDirectoryPath, true);

                            print("Update download completed");
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
            File.AppendAllText(LogFilePath, NormalizeText);
        }
    }
}
