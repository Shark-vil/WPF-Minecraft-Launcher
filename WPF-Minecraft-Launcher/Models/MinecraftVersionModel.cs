﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    [Serializable]
    public class MinecraftVersionModel
    {
        public bool success { get; set; }
        public MinecraftVersionResponseModel response { get; set; }
    }

    [Serializable]
    public class MinecraftVersionResponseModel
    {
        public int id { get; set; }
        public string tag { get; set; }
        public string path { get; set; }
        public string hash { get; set; }
        public int actual { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string link { get; set; }
    }
}
