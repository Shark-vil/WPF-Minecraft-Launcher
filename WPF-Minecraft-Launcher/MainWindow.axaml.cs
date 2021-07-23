using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using WPF_Minecraft_Launcher.Models;
using Avalonia.Threading;
using System.Diagnostics;
using System;
using System.IO;
using WPF_Minecraft_Launcher.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher
{
    public partial class MainWindow : Window
    {
        internal MainWindowModel Context = new MainWindowModel();
        internal string MinecraftProcessName;

        private SettingsWindow SettingsWindowUI;
        private TextBox TextBox_Logs;
        private TextBox TextBox_Username;
        private TextBox TextBox_Password;
        private Button Button_Play;

        public MainWindow()
        {
            InitializeComponent();

            bool mainThreadRegister = ThreadChecker.IsMainThread;

            Global.MainWindowUI = this;
            Global.LauncherConfigInit();

            DataContext = Context;

            TextBox_Logs = this.FindControl<TextBox>("TextBox_Logs");
            TextBox_Username = this.FindControl<TextBox>("TextBox_Username");
            TextBox_Password = this.FindControl<TextBox>("TextBox_Password");
            Button_Play = this.FindControl<Button>("Button_Play");

            this.SwitchTextBoxActiveByValidate();

#if DEBUG
            this.AttachDevTools();
#else
            this.FindControl<Grid>("Grid_Container").ShowGridLines = false;
#endif

            CheckLauncherUpdate();
        }

        private void CheckLauncherUpdate()
        {
            string VersionFilePath = Path.Combine(Global.ConfigPath, "version.dat");

            if (!File.Exists(VersionFilePath))
                StartLauncherUpdate();
            else
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        string JsonData = client.DownloadString(Global.GetApiAddress("launcherversion/get_actual"));

                        var LauncherObject = JsonConvert.DeserializeObject<LauncherModel>(JsonData);

                        if (LauncherObject == null)
                            throw new Exception("Failed to convert launcher model to equal object");

                        string ServerVersion = LauncherObject.response.tag;
                        string CurrentVersion = File.ReadAllText(VersionFilePath);

                        if (CurrentVersion == ServerVersion)
                            WriteTextToLogBox("The version is the same, no update is required");
                        else
                        {
                            WriteTextToLogBox("The version does not match, an update is required");
                            StartLauncherUpdate();
                        }
                    }
                }
                catch(Exception ex)
                {
                    WriteTextToLogBox("An error occurred while trying to check the launcher version");
                    WriteTextToLogBox(ex.ToString());
                }
            }
        }

        private void StartLauncherUpdate()
        {
            try
            {
                string UpdateFilePath = Path.Combine(Global.ApplicationPath, "update");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    UpdateFilePath = Path.Combine(UpdateFilePath, "update.exe");
                else
                {
                    UpdateFilePath = Path.Combine(UpdateFilePath, "update");
                    Process.Start("chmod", $"755 {UpdateFilePath}");
                }

                Process.Start(UpdateFilePath);

                this.Close();
            }
            catch(Exception ex)
            {
                Global.LauncherLogger?.Write(ex.ToString(), "ERROR");
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        internal void WriteTextToLogBox(object AnyData)
        {
            string? Text = Convert.ToString(AnyData);

            if (Text == null || Text.Length == 0)
                return;

            string CurrentDateTimeText = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            string TextFormated = string.Format("[{0}] {1}", CurrentDateTimeText, Text);

            try
            {
                var response = JsonConvert.DeserializeObject<WebResponseModel>(Text);
                if (response != null)
                    TextFormated = string.Format("[{0}] {1} - {2}", CurrentDateTimeText, response.code, response.message);
            }
            catch { }

            Global.LauncherLogger?.Write(TextFormated);

            int MaxTextLenght = 300;
            int TextLenght = TextFormated.Length;
            Context.Logs += TextFormated.Substring(0, (TextLenght <= MaxTextLenght ? TextLenght : MaxTextLenght)) + "\n";

            if (ThreadChecker.IsMainThread)
                TextBox_Logs.CaretIndex = int.MaxValue;
            else
                Dispatcher.UIThread.InvokeAsync(() => TextBox_Logs.CaretIndex = int.MaxValue, DispatcherPriority.MinValue);
        }

        private void OnAuthorizateClick(object sender, RoutedEventArgs e)
        {
            if (MinecraftProcessName != null && Process.GetProcessesByName(MinecraftProcessName).Length > 0)
                return;

            Context.Logs = "";
            SwitchActiveUI(false);

            var UserProfile = new Profile(Context.UserName, Context.UserPassword);
            var MinecraftGameStarter = new GameStarter(this, UserProfile);
            MinecraftGameStarter.Launch();
        }

        private void OnOpenSettings(object sender, RoutedEventArgs e)
        {
            if (SettingsWindowUI != null)
                return;

            SettingsWindowUI = new SettingsWindow();
            SettingsWindowUI.Closed += (object? sender, EventArgs e) => SettingsWindowUI = null;
            SettingsWindowUI.Show();
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

        internal void SwitchActiveUI(bool active)
        {
            if (ThreadChecker.IsMainThread)
            {
                this.Button_Play.IsEnabled = active;
                this.SwitchTextBoxActive(active);
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => SwitchActiveUI(active));

            if (active)
                ResetAllProgressBars();
        }

        internal void SwitchShowWindow(bool showing)
        {
            if (ThreadChecker.IsMainThread)
            {
                if (showing)
                    this.Show();
                else
                    this.Hide();
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => this.SwitchShowWindow(showing));
        }

        internal void SwitchTextBoxActiveByValidate()
        { 
            var MLogin = new MLauncherLogin();

            bool IsValid = (MLogin.Validate().Result == CmlLib.Core.Auth.MLoginResult.Success);
            if (!IsValid)
                IsValid = (MLogin.Refresh().Result == CmlLib.Core.Auth.MLoginResult.Success);

            this.SwitchTextBoxActive(!IsValid);
        }

        internal void SwitchTextBoxActive(bool showing)
        {
            if (ThreadChecker.IsMainThread)
            {
                this.TextBox_Username.IsEnabled = showing;
                this.TextBox_Password.IsEnabled = showing;

                if (showing)
                    Context.StartButtonText = "Авторизация";
                else
                    Context.StartButtonText = "Запуск";
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => this.SwitchTextBoxActive(showing));
        }

        internal void ResetAllProgressBars()
        {
            if (ThreadChecker.IsMainThread)
            {
                Context.FileChangeValue = 0;
                Context.ProgressChangeValue = 0;
            }
            else
                Dispatcher.UIThread.InvokeAsync(() => ResetAllProgressBars());
        }

        internal void CloseMinecraftProcess()
        {
            SwitchShowWindow(true);
            SwitchActiveUI(true);
            ResetAllProgressBars();

            SwitchTextBoxActiveByValidate();

            Process process = GameStarter.GameProcessObject.gameProcess;
            if (process != null)
                process.Kill();

            WriteTextToLogBox("Game closed");
        }
    }
}
