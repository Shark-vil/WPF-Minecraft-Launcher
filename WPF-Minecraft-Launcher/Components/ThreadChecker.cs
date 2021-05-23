using System;

namespace WPF_Minecraft_Launcher.Components
{
    public class ThreadChecker
    {
        [ThreadStatic]
        public static readonly bool IsMainThread = true;
    }
}
