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
        internal SettingsWindowModel content = new SettingsWindowModel();

        public SettingsWindow()
        {
            InitializeComponent();

            this.DataContext = content;
            this.Closed += SettingsWindow_Closed;

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void SettingsWindow_Closed(object? sender, System.EventArgs e)
        {
            string config_path = Path.Combine(Global.ConfigPath, "config.json");
            string config_json = JsonConvert.SerializeObject(Global.LauncherConfig, Formatting.Indented);
            File.WriteAllText(config_path, config_json);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
