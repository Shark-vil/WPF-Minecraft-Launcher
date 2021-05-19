using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    [Serializable]
    public class DelAddonModel
    {
        public bool success { get; set; }
        public List<DelAddonResponseModel> response { get; set; }
    }

    [Serializable]
    public class DelAddonResponseModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string hash { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}
