using Newtonsoft.Json;
using System.Net;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class MinecraftVersion
    {
        public static MinecraftVersionModel? GetVersion()
        {
            string get_version_url = Global.GetApiAddress("minecraftversion/get_actual");

            try
            {
                using (var wc = new WebClient())
                {
                    string json = wc.DownloadString(get_version_url);
                    var minecraft_version = JsonConvert.DeserializeObject<MinecraftVersionModel>(json);

                    return minecraft_version;
                }
            }
            catch { }

            return null;
        }
    }
}
