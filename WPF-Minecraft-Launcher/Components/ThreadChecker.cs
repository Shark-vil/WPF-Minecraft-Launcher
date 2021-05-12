using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Components
{
    public class ThreadChecker
    {
        [ThreadStatic]
        public static readonly bool IsMainThread = true;
    }
}
