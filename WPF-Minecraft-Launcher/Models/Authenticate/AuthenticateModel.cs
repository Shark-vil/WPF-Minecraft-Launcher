using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models.Authenticate
{
    [Serializable]
    public class AuthenticateModel
    {
        public string accessToken { get; set; }
        public AuthenticateSelectedProfileModel selectedProfile { get; set; }

        public static AuthenticateModel Convert(string json) => JsonConvert.DeserializeObject<AuthenticateModel>(json);
    }

    [Serializable]
    public class AuthenticateSelectedProfileModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
