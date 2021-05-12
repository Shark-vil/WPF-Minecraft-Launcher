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

namespace WPF_Minecraft_Launcher
{
    public partial class MainWindow : Window
    {
        internal MainWindowModel content = new MainWindowModel();
        internal string processName;

        private TextBox TextBox_Logs;
        private Button Button_Play;

        public MainWindow()
        {
            InitializeComponent();

            bool mainThreadRegister = ThreadChecker.IsMainThread;

            var fileManager = new OpenFileDialog();
            fileManager.AllowMultiple = false;

            Logger.Init();
            Global.LauncherConfigInit();

            DataContext = content;

            TextBox_Logs = this.FindControl<TextBox>("TextBox_Logs");
            Button_Play = this.FindControl<Button>("Button_Play");

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

            content.logs += textFormat + "\n";

//#if DEBUG
//            Logger.Write(textFormat);
//#endif

            if (ThreadChecker.IsMainThread)
                TextBox_Logs.CaretIndex = int.MaxValue;
            else
                Dispatcher.UIThread.InvokeAsync(() => TextBox_Logs.CaretIndex = int.MaxValue, DispatcherPriority.MinValue);
        }

        private void OnAuthorizateClick(object sender, RoutedEventArgs e)
        {
            if (content.username == null || content.username.Length == 0)
                return;

            if (processName != null && Process.GetProcessesByName(processName).Length > 0)
                return;

            var minecraft = new GameStarter(this);
            minecraft.Launch();
        }

        internal void switchUiActive(bool active)
        {
            if (ThreadChecker.IsMainThread)
                this.Button_Play.IsEnabled = active;
            else
                Dispatcher.UIThread.InvokeAsync(() => this.Button_Play.IsEnabled = active);

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
            {
                if (showing)
                    Dispatcher.UIThread.InvokeAsync(() => this.Show());
                else
                    Dispatcher.UIThread.InvokeAsync(() => this.Hide());
            }
        }

        internal void resetProgress()
        {
            if (ThreadChecker.IsMainThread)
            {
                content.fileChangedValue = 0;
                content.progressChangedValue = 0;
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() => content.fileChangedValue);
                Dispatcher.UIThread.InvokeAsync(() => content.progressChangedValue = 0);
            }
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
