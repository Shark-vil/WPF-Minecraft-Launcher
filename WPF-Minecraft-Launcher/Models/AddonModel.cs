using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    [Serializable]
    public class AddonModel
    {
        public bool success { get; set; }
        public List<AddonResponseModel> response { get; set; }
    }

    [Serializable]
    public class AddonResponseModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string hash { get; set; }
        public string path { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string link { get; set; }
    }
}
