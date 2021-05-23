using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using System.IO;
using WPF_Minecraft_Launcher.Components;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher
{
    public partial class SettingsWindow : Window
    {
        internal SettingsWindowModel Context = new SettingsWindowModel();

        public SettingsWindow()
        {
            InitializeComponent();

            this.DataContext = Context;
            this.Closed += SettingsWindow_Closed;

#if DEBUG
            this.AttachDevTools();
#else
            this.FindControl<Grid>("Grid_Container").ShowGridLines = false;
#endif
        }

        private void SettingsWindow_Closed(object? sender, System.EventArgs e)
        {
            string ConfigFilePath = Path.Combine(Global.ConfigPath, "config.json");
            string ConfigJsonText = JsonConvert.SerializeObject(Global.LauncherConfig, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, ConfigJsonText);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
