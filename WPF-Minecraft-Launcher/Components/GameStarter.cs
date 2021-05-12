using Avalonia.Threading;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Downloader;
using CmlLib.Core.Version;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class GameStarter
    {
        private MainWindow window;
        private MainWindowModel content;
        private Thread gameThread;
        private Thread processCheckerThread;
        private MSession userMinecraftSession;
        private MinecraftPath minecraftPath;
        internal static GameModel gameModel = new GameModel();

        public GameStarter(MainWindow window)
        {
            this.window = window;
            this.content = window.content;
        }

        internal void Launch()
        {
            content.fileChangedValue = 0;
            content.progressChangedValue = 0;

            userMinecraftSession = MSession.GetOfflineSession(content.username);
            minecraftPath = new MinecraftPath(Global.LauncherConfig.MinecraftPath);

            window.switchUiActive(false);

            gameThread = new Thread(OnLaunchMinecraft);
            gameThread.IsBackground = true;
            gameThread.Priority = ThreadPriority.Highest;
            gameThread.Start();

            gameModel.minecraftPath = minecraftPath;
            gameModel.session = userMinecraftSession;
            gameModel.starterThread = gameThread;
        }

        private void OnLaunchMinecraft()
        {
            var launchOption = new MLaunchOption
            {
                MaximumRamMb = 1024,
                Session = userMinecraftSession,
                GameLauncherName = "Minecraft-Client-Pipbuck",
                VersionType = "Minecraft-Client-Pipbuck",
                GameLauncherVersion = "1.0.0",
                ServerIp = "95.216.122.173",
                ServerPort = 25565,
            };

            var launcher = new CMLauncher(minecraftPath);

            launcher.FileChanged += (e) =>
            {
                string FileKind = e.FileKind.ToString();
                string FileName = e.FileName;
                int ProgressedFileCount = e.ProgressedFileCount;
                int TotalFileCount = e.TotalFileCount;
                string text = string.Format("[{0}] {1} - {2}/{3}", FileKind, FileName, ProgressedFileCount, TotalFileCount);

                content.fileChangedMin = 0;
                content.fileChangedMax = TotalFileCount;
                content.fileChangedValue = ProgressedFileCount;

                if (FileKind.ToLower() != "resource" || FileName.Length != 0)
                    window.AddLineToLog(text);

                if (ProgressedFileCount == TotalFileCount)
                    window.AddLineToLog(text);
            };

            launcher.ProgressChanged += (s, e) =>
            {
                content.progressChangedMin = 0;
                content.progressChangedMax = 100;
                content.progressChangedValue = e.ProgressPercentage;
            };

            launcher.LogOutput += (s, e) => window.AddLineToLog(e);

            var process = launcher.CreateProcess(Global.LauncherConfig.MinecraftVersion, launchOption);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.ErrorDialog = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            gameModel.gameProcess = process;

            window.AddLineToLog("Starting the game process.");

            Logger.Write(process.StartInfo.Arguments, "PARAMS");

            if (process.Start())
            {
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                window.processName = process.ProcessName;

                processCheckerThread = new Thread(ProcessChecker);
                processCheckerThread.IsBackground = true;
                processCheckerThread.Priority = ThreadPriority.Lowest;
                processCheckerThread.Start();

                window.AddLineToLog("The game watcher process has started.");

                Dispatcher.UIThread.InvokeAsync(() => window.Hide());
            }
            else
            {
                window.AddLineToLog("Error! Failed to start the game! Please check the file - pipbuck-launcher.log");
                window.switchUiActive(true);
            }

            window.resetProgress();
        }

        private void ProcessChecker()
        {
            bool isLaunched = false;

            while (true)
            {
                int ProcessLength = Process.GetProcessesByName(window.processName).Length;

                if (isLaunched)
                {
                    if (ProcessLength == 0)
                    {
                        window.gameCLosed();
                        break;
                    }
                }
                else if (ProcessLength > 0)
                    isLaunched = true;

                Task.Delay(1000);
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? data = e.Data?.ToString();

            if (data == null)
                return;

            window.AddLineToLog("System message added to - pipbuck-launcher.log");

            Logger.Write(data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? data = e.Data?.ToString();

            if (data == null)
                return;

            window.AddLineToLog("An unexpected error has occurred, check the file - pipbuck-launcher.log");

            Logger.Write(data, "ERROR");
        }
    }
}
