using CmlLib.Core;
using CmlLib.Core.Auth;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WPF_Minecraft_Launcher.Models;

namespace WPF_Minecraft_Launcher.Components
{
    public class GameStarter
    {
        private MainWindow MainWindowUI;
        private MainWindowModel MainWindowContext;
        private Thread GameThread;
        private MSession MinecraftUserSession;
        private MinecraftPath GameDirectoryPath;
        private Profile UserProfile;
        private MLauncherLogin MinecraftLauncherAuthorizer = new MLauncherLogin();
        internal static GameModel GameProcessObject = new GameModel();

        public GameStarter(MainWindow window, Profile profile)
        {
            this.UserProfile = profile;
            this.MainWindowUI = window;
            this.MainWindowContext = window.Context;
        }

        internal void Launch()
        {
            Global.MainWindowUI.SwitchActiveUI(false);

            MLoginResponse Response = MinecraftLauncherAuthorizer.TryAutoLogin();
            if (Response.Result != MLoginResult.Success)
            {
                Global.MainWindowUI.SwitchActiveUI(true);

                if (UserProfile.UserName.Length != 0 && UserProfile.UserPassword.Length != 0)
                    Launch(UserProfile.UserName, UserProfile.UserPassword);
            }
            else
                Launch(Response);
        }

        internal void Launch(string username, string password)
        {
            MLoginResponse Response = MinecraftLauncherAuthorizer.Authenticate(username, password);
            Launch(Response);
        }

        internal void Launch(MLoginResponse Response)
        {
            MainWindowContext.FileChangeValue = 0;
            MainWindowContext.ProgressChangeValue = 0;

            //userMinecraftSession = MSession.GetOfflineSession(content.username);

            if (Response.IsSuccess)
            {
                Global.MainWindowUI.SwitchActiveUI(false);

                MinecraftUserSession = Response.Session;
                UserProfile.AccessToken = Response.Session.AccessToken;

                GameDirectoryPath = new MinecraftPath(Global.MinecraftPath);

                GameThread = new Thread(OnLaunchMinecraft);
                GameThread.IsBackground = true;
                GameThread.Priority = ThreadPriority.Normal;
                GameThread.Start();

                GameProcessObject.minecraftPath = GameDirectoryPath;
                GameProcessObject.session = MinecraftUserSession;
                GameProcessObject.starterThread = GameThread;
            }
            else
                Global.MainWindowUI.SwitchActiveUI(true);
        }

        private void OnLaunchMinecraft()
        {
            var JavaDownloaderService = new JavaRuntimeDownloader();
            var AddonsDownloaderService = new AddonsDownloader();

            JavaDownloaderService.ProgressChanged += (s, e) =>
            {
                MainWindowContext.ProgressChangeMinimum = 0;
                MainWindowContext.ProgressChangeMaximum = 100;
                MainWindowContext.ProgressChangeValue = e.ProgressPercentage;
                MainWindowContext.ProgressChangeText = $"{MainWindowContext.ProgressChangeValue}/{MainWindowContext.ProgressChangeMaximum}%";
            };
            JavaDownloaderService.DownloadCompleted = (string JavaRuntimeDirectoryPath) => Launcher_Start(JavaRuntimeDirectoryPath);

            AddonsDownloaderService.ProgressChanged += (s, e) =>
            {
                MainWindowContext.ProgressChangeMinimum = 0;
                MainWindowContext.ProgressChangeMaximum = 100;
                MainWindowContext.ProgressChangeValue = e.ProgressPercentage;
                MainWindowContext.ProgressChangeText = $"{MainWindowContext.ProgressChangeValue}/{MainWindowContext.ProgressChangeMaximum}%";
            };
            AddonsDownloaderService.DownloadCompleted = () => JavaDownloaderService.CheckJava();

            Task.Run(() => AddonsDownloaderService.Download());
        }

        private void Launcher_Start(string javapath)
        {
            MinecraftVersionModel Version = MinecraftVersion.GetVersion();
            if (Version == null)
            {
                MainWindowUI.WriteTextToLogBox("Failed to get the latest version.");
                MainWindowUI.SwitchActiveUI(true);
                return;
            }

            string InjectorFilePath = Path.Combine(Global.MinecraftPath, "authlib.jar");
            if (!File.Exists(InjectorFilePath))
                File.WriteAllBytes(InjectorFilePath, Properties.Resources.authlib);

            int MinimumRAM = 2048;
            int MaximumRAM = Global.LauncherConfig.MaxRAM < MinimumRAM ? MinimumRAM : Global.LauncherConfig.MaxRAM;

            var MinecraftOptions = new MLaunchOption
            {
                Session = MinecraftUserSession,
                GameLauncherName = "Minecraft-Client-Pipbuck",
                VersionType = "Minecraft-Client-Pipbuck",
                GameLauncherVersion = "1.0.0",
                JavaPath = javapath,
                JVMArguments = new string[]
                {
                    $"-javaagent:{InjectorFilePath}=" + Global.LauncherConfig.AuthserverAddress,
                    "-Dauthlibinjector.debug",
                    "-Dauthlibinjector.noLogFile",
                    $"-Xms{MinimumRAM}m",
                    $"-Xmx{MaximumRAM}m"
                },
                ServerIp = (Global.LauncherConfig.ServerIP.Length != 0) ? Global.LauncherConfig.ServerIP : null,
                ServerPort = Global.LauncherConfig.ServerPort,
            };

            var LauncherService = new CMLauncher(GameDirectoryPath);

            LauncherService.FileChanged += (e) =>
            {
                string FileKind = e.FileKind.ToString();
                string FileName = e.FileName;
                int ProgressedFileCount = e.ProgressedFileCount;
                int TotalFileCount = e.TotalFileCount;
                string text = string.Format("[{0}] {1} - {2}/{3}", FileKind, FileName, ProgressedFileCount, TotalFileCount);

                MainWindowContext.FileChangeMinimum = 0;
                MainWindowContext.FileChangeMaximum = TotalFileCount;
                MainWindowContext.FileChangeValue = ProgressedFileCount;

                if (FileKind.ToLower() != "resource" || FileName.Length != 0)
                    MainWindowUI.WriteTextToLogBox(text);

                if (ProgressedFileCount == TotalFileCount)
                    MainWindowUI.WriteTextToLogBox(text);

                MainWindowContext.FileChangeText = text;
            };

            LauncherService.ProgressChanged += (s, e) =>
            {
                MainWindowContext.ProgressChangeMinimum = 0;
                MainWindowContext.ProgressChangeMaximum = 100;
                MainWindowContext.ProgressChangeValue = e.ProgressPercentage;
                MainWindowContext.ProgressChangeText = $"{MainWindowContext.ProgressChangeValue}/{MainWindowContext.ProgressChangeMaximum}%";
            };

            LauncherService.LogOutput += (s, e) => Global.LauncherLogger.Write(e);

            Process GameProcess;

            string ActualMinecraftVersion = Version.response.minecraft_version;
            string ActualForgeVersion = Version.response.forge_version;

            if (ActualMinecraftVersion.Length != 0)
            {
                MainWindowUI.WriteTextToLogBox("The launcher will download the required packages, please wait.");

                if (ActualForgeVersion.Length != 0)
                    GameProcess = LauncherService.CreateProcess(ActualMinecraftVersion, ActualForgeVersion, MinecraftOptions);
                else
                    GameProcess = LauncherService.CreateProcess(ActualMinecraftVersion, MinecraftOptions);

                Process_Start(GameProcess);
            }
            else
            {
                MainWindowUI.WriteTextToLogBox("Error. Invalid version information, empty string.");
                MainWindowUI.SwitchActiveUI(true);
            }
        }

        private void Process_Start(Process GameProcess)
        {
            GameProcess.StartInfo.UseShellExecute = false;
            GameProcess.StartInfo.RedirectStandardOutput = true;
            GameProcess.StartInfo.RedirectStandardError = true;
            GameProcess.StartInfo.ErrorDialog = true;
            GameProcess.EnableRaisingEvents = true;
            GameProcess.ErrorDataReceived += Process_ErrorDataReceived;
            GameProcess.OutputDataReceived += Process_OutputDataReceived;
            GameProcess.Exited += Process_Exited;
            GameProcessObject.gameProcess = GameProcess;

            MainWindowUI.WriteTextToLogBox("Starting the game process.");

            Global.LauncherLogger.Write(GameProcess.StartInfo.Arguments, "PARAMS");

            if (GameProcess.Start())
            {
                GameProcess.BeginErrorReadLine();
                GameProcess.BeginOutputReadLine();

                MainWindowUI.MinecraftProcessName = GameProcess.ProcessName;

#if (!DEBUG)
                Dispatcher.UIThread.InvokeAsync(() => window.Hide());
#endif
            }
            else
            {
                MainWindowUI.WriteTextToLogBox("Error! Failed to start the game! Please check the file - pipbuck-launcher.log");
                MainWindowUI.SwitchActiveUI(true);
            }

            MainWindowUI.ResetAllProgressBars();
        }

        private void Process_Exited(object? sender, EventArgs e) => MainWindowUI.CloseMinecraftProcess();

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? GameLog = e.Data?.ToString();

            if (GameLog == null)
                return;

            Global.GameLogger.Write(GameLog);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string? GameErrorLog = e.Data?.ToString();

            if (GameErrorLog == null)
                return;

            Global.GameErrorsLogger.Write(GameErrorLog, "ERROR");
        }
    }
}
