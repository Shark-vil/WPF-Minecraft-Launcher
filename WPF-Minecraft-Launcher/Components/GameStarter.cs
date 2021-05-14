using Avalonia.Threading;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Downloader;
using CmlLib.Core.Version;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private MSession userMinecraftSession;
        private MinecraftPath minecraftPath;
        private Profile profile;
        private MLauncherLogin launcherLogin = new MLauncherLogin();
        internal static GameModel gameModel = new GameModel();

        public GameStarter(MainWindow window, Profile profile)
        {
            this.profile = profile;
            this.window = window;
            this.content = window.content;
        }

        internal void Launch()
        {
            MLoginResponse response = launcherLogin.TryAutoLogin();
            if (response.Result != MLoginResult.Success)
            {
                if (profile.username.Length != 0 && profile.password.Length != 0)
                    Launch(profile.username, profile.password);
            }
            else
                Launch(response);
        }

        internal void Launch(string username, string password)
        {
            MLoginResponse response = launcherLogin.Authenticate(username, password);
            Launch(response);
        }

        internal void Launch(MLoginResponse response)
        {
            content.fileChangedValue = 0;
            content.progressChangedValue = 0;

            //userMinecraftSession = MSession.GetOfflineSession(content.username);

            if (response.IsSuccess)
            {
                userMinecraftSession = response.Session;
                profile.token = response.Session.AccessToken;

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
                JVMArguments = new string[]
                {
                    $"-javaagent:{Global.LauncherConfig.MinecraftPath}"
                        + "/authlib-injector-1.1.34.jar=" + Global.LauncherConfig.SiteAddress
                }
                //ServerIp = "95.216.122.173",
                //ServerPort = 25565,
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
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Exited += Process_Exited;
            gameModel.gameProcess = process;

            window.AddLineToLog("Starting the game process.");

            Global.LauncherLogger.Write(process.StartInfo.Arguments, "PARAMS");

            if (process.Start())
            {
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                window.processName = process.ProcessName;

                window.AddLineToLog("The game watcher process has started.");

#if (!DEBUG)
                Dispatcher.UIThread.InvokeAsync(() => window.Hide());
#endif
            }
            else
            {
                window.AddLineToLog("Error! Failed to start the game! Please check the file - pipbuck-launcher.log");
                window.switchUiActive(true);
            }

            window.resetProgress();
        }

        private void Process_Exited(object? sender, EventArgs e) => window.gameCLosed();

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? data = e.Data?.ToString();

            if (data == null)
                return;

            Global.GameLogger.Write(data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? data = e.Data?.ToString();

            if (data == null)
                return;

            Global.GameErrorsLogger.Write(data, "ERROR");
        }
    }
}
