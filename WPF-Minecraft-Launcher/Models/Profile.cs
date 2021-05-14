using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    public class Profile
    {
        internal string username;
        internal string password;
        internal string token;

        public Profile(string username = "", string password = "", string token = "")
        {
            this.username = username;
            this.password = password;
            this.token = token;
        }
    }
}
