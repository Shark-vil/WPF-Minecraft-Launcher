using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CmlLib.Core;
using CmlLib.Core.Auth;
using Avalonia.Interactivity;
using WPF_Minecraft_Launcher.Models;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Threading;
using System.Diagnostics;
using System;
using System.IO;
using WPF_Minecraft_Launcher.Components;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace WPF_Minecraft_Launcher
{
    public partial class MainWindow : Window
    {
        internal MainWindowModel content = new MainWindowModel();
        internal string processName;

        private SettingsWindow settingsWindow;
        private TextBox TextBox_Logs;
        private TextBox TextBox_Username;
        private TextBox TextBox_Password;
        private Button Button_Play;

        public MainWindow()
        {
            InitializeComponent();

            bool mainThreadRegister = ThreadChecker.IsMainThread;

            Global.mainWindow = this;
            Global.LauncherConfigInit();

            if (!Directory.Exists(Global.MinecraftPath))
                Directory.CreateDirectory(Global.MinecraftPath);

            if (!Directory.Exists(Global.ConfigPath))
                Directory.CreateDirectory(Global.ConfigPath);

            if (!Directory.Exists(Global.CachePath))
                Directory.CreateDirectory(Global.CachePath);

            DataContext = content;

            TextBox_Logs = this.FindControl<TextBox>("TextBox_Logs");
            TextBox_Username = this.FindControl<TextBox>("TextBox_Username");
            TextBox_Password = this.FindControl<TextBox>("TextBox_Password");
            Button_Play = this.FindControl<Button>("Button_Play");

            switchTextBoxActive(false);

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        internal void AddLineToLog(string text)
        {
            if (text == null || text.Length == 0)
                return;

            string dateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            string textFormat = string.Format("[{0}] {1}", dateTime, text);

            WebResponseModel? response = null;

            try
            {
                response = JsonConvert.DeserializeObject<WebResponseModel>(text);

                if (response != null)
                    textFormat = string.Format("[{0}] {1} - {2}", dateTime, response.code, response.message);
            }
            catch { }

#if DEBUG
            Global.LauncherLogger.Write(textFormat);
#endif

            int maxLenght = 300;
            int textLenght = textFormat.Length;
            content.logs += textFormat.Substring(0, (textLenght <= maxLenght ? textLenght : maxLenght)) + "\n";

            if (ThreadChecker.IsMainThread)
                TextBox_Logs.CaretIndex = int.MaxValue;
            else
                Dispatcher.UIThread.InvokeAsync(() => TextBox_Logs.CaretIndex = int.MaxValue, DispatcherPriority.MinValue);
        }

        private void OnAuthorizateClick(object sender, RoutedEventArgs e)
        {
            if (processName != null && Process.GetProcessesByName(processName).Length > 0)
                return;

            content.logs = "";

            var profile = new Profile(content.username, content.password);
            var minecraft = new GameStarter(this, profile);
            minecraft.Launch();
        }

        private void OnOpenSettings(object sender, RoutedEventArgs e)
        {
            if (settingsWindow != null)
                return;

            settingsWindow = new SettingsWindow();
            settingsWindow.Closed += (object? sender, EventArgs e) => settingsWindow = null;
            settingsWindow.Show();
        }

        private void OnOpenSite(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(Global.LauncherConfig.SiteAddress) { UseShellExecute = true });
        }

        private void OnOpenSkinSite(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(Global.LauncherConfig.SiteAddress + "/profile/skin") { UseShellExecute = true });
        }

        private void OnCloseApplication(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        internal void switchUiActive(bool active)
        {
            if (ThreadChecker.IsMainThread)
            {
                this.Button_Play.IsEnabled = active;
                this.TextBox_Username.IsEnabled = active;
                this.TextBox_Password.IsEnabled = active;
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => switchUiActive(active));

            if (active)
                resetProgress();
        }

        internal void switchWindowShow(bool showing)
        {
            if (ThreadChecker.IsMainThread)
            {
                if (showing)
                    this.Show();
                else
                    this.Hide();
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => this.switchWindowShow(showing));
        }

        internal void switchTextBoxActive(bool showing)
        {
            if (ThreadChecker.IsMainThread)
            {
                this.TextBox_Username.IsEnabled = showing;
                this.TextBox_Password.IsEnabled = showing;
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => this.switchTextBoxActive(showing));
        }

        internal void resetProgress()
        {
            if (ThreadChecker.IsMainThread)
            {
                content.fileChangedValue = 0;
                content.progressChangedValue = 0;
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => resetProgress());
        }

        internal void gameCLosed()
        {
            switchWindowShow(true);
            switchUiActive(true);
            resetProgress();

            Process process = GameStarter.gameModel.gameProcess;
            if (process != null)
                process.Kill();

            AddLineToLog("Game closed");
        }
    }
}
