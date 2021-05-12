using CmlLib.Core;
using CmlLib.Core.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    public class GameModel
    {
        internal MSession session;
        internal MinecraftPath minecraftPath;
        internal Thread starterThread;
        internal Process gameProcess;
    }
}
