using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    public class Profile
    {
        internal string UserName;
        internal string UserPassword;
        internal string AccessToken;

        public Profile(string username = "", string password = "", string token = "")
        {
            this.UserName = username;
            this.UserPassword = password;
            this.AccessToken = token;
        }
    }
}
