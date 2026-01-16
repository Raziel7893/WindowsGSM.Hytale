using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;

namespace WindowsGSM.Plugins
{
    public class Hytale
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Hytale", // WindowsGSM.XXXX
            author = "raziel7893",
            description = "WindowsGSM plugin for supporting Hytale Dedicated Server",
            version = "1.0.2",
            url = "https://github.com/Raziel7893/WindowsGSM.Hytale", // Github repository link (Best practice) TODO
            color = "#34FFeb" // Color Hex
        };

        // - Standard Constructor and properties
        public Hytale(ServerConfig _serverData) => serverData = _serverData;

        // - Game server Fixed variables
        ServerConfig serverData;
        public string Error { get; set; }
        public string Notice { get; set; }

        public string StartPath = "Server\\HytaleServer.jar"; //TODO: check correct path
        public string Defaultmap = "Assets.zip"; // Default map name
        public string FullName = "Hytale Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation

        // - Game server default values
        public string Port = "5520"; // Default port

        public string Additional = $""; // Additional server start parameter

        // TODO: Following options are not supported yet, as ther is no documentation of available options
        public string Maxplayers = "16"; // Default maxplayers        
        public string QueryPort = "5520"; // Default query port. This is the port specified in the Server Manager in the client UI to establish a server connection.

        // TODO: Undisclosed method
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()

        //Hytale specifics
        public const string DownloaderUrl = "https://downloader.hytale.com/hytale-downloader.zip";
        public const string JreApiUrl = "https://api.github.com/repos/adoptium/temurin25-binaries/releases/latest";
        //        public const string JreUrl = "https://github.com/adoptium/temurin25-binaries/releases/download/jdk-25.0.1%2B8/OpenJDK25U-jre_x64_windows_hotspot_25.0.1_8.zip";
        public const string JreRootPath = "JRE25";
        public const string InstallerFolder = "installer";

        public string JreZip = Path.Combine(InstallerFolder, "jre25.zip");
        public string HytaleZip = Path.Combine(InstallerFolder, "Hytale.zip");
        public string HytaleDownloaderZip = Path.Combine(InstallerFolder, "hytale-downloader.zip");
        public string HytaleDownloader = Path.Combine(InstallerFolder, "hytale-downloader-windows-amd64.exe");

        public string HytaleVersion = Path.Combine(InstallerFolder, "hytaleVersion.txt");
        public string JreVersion = Path.Combine(InstallerFolder, "hytaleVersion.txt");

        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string hytaleZipPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleZip);
            string shipExePath = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, StartPath);

            if (File.Exists(hytaleZipPath))
            {
                var fileReader = new StreamReader(hytaleZipPath).BaseStream;
                ZipFile.ExtractToDirectory(fileReader, ServerPath.GetServersServerFiles(serverData.ServerID), true);
                fileReader.Close();
                File.Delete(hytaleZipPath);
            }

            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }

            //prepare java parameters, maybe from a cfg? Lets try ServerstartParam first
            var paramSb = new StringBuilder();
            paramSb.Append(serverData.ServerParam);
            paramSb.Append($" -XX:AOTCache={ServerPath.GetServersServerFiles(serverData.ServerID, "Server", "HytaleServer.aot")}");
            paramSb.Append($" -jar {shipExePath}");
            paramSb.Append($" --assets {ServerPath.GetServersServerFiles(serverData.ServerID, serverData.ServerMap)}");
            paramSb.Append($" --bind {serverData.ServerIP}:{serverData.ServerPort}");

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = false,
                    WorkingDirectory = ServerPath.GetServersServerFiles(serverData.ServerID),
                    FileName = GetJavaPath(),
                    Arguments = paramSb.ToString(),
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (serverData.EmbedConsole)
            {
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                var serverConsole = new ServerConsole(serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
            }

            // Start Process
            try
            {
                p.Start();
                if (serverData.EmbedConsole)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }

        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                Functions.ServerConsole.SendWaitToMainWindow("^c");
                p.WaitForExit(2000);
                if (!p.HasExited)
                    p.Kill();
            });
        }

        public async Task<Process> Install()
        {
            string tmpInstallPath = ServerPath.GetServersServerFiles(serverData.ServerID, InstallerFolder);
            string hytaleInstallerZipPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleDownloaderZip);
            string hytaleInstallerPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleDownloader);
            string hytaleZip = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleZip);

            Directory.CreateDirectory(tmpInstallPath);
            Directory.CreateDirectory(ServerPath.GetServersServerFiles(serverData.ServerID, JreRootPath));
            File.Create(ServerPath.GetServersServerFiles(serverData.ServerID, HytaleVersion)).Close();
            File.Create(ServerPath.GetServersServerFiles(serverData.ServerID, JreVersion)).Close();

            //Get Java
            await DownloadCurrentJre();

            //skip downloader for debugging
            if (File.Exists(".\\Hytale.zip"))
            {
                File.Copy(".\\Hytale.zip", hytaleZip);
                await Task.Delay(2000);
                return null;
            }

            //Get Hytale Downlaoder
            if (!await DownloadFileAsync(DownloaderUrl, hytaleInstallerZipPath)) return null;
            ZipFile.ExtractToDirectory(new StreamReader(hytaleInstallerZipPath).BaseStream, ServerPath.GetServersServerFiles(serverData.ServerID, tmpInstallPath));

            return StartProcessAsync(hytaleInstallerPath, $" -download-path {hytaleZip}");
            //the hytale.zip will not be extracted here, this will be done in CreateServerCfg as the returning of the process is needed to pass on the output of the login page
        }

        public string GetJavaPath()
        {
            var subdirs = Directory.GetDirectories(ServerPath.GetServersServerFiles(serverData.ServerID, JreRootPath)).ToList();
            subdirs.Sort();

            string javaRoot = subdirs.Last();
            return Path.Combine(javaRoot, "bin\\java.exe");
        }

        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            string tmpInstallPath = ServerPath.GetServersServerFiles(serverData.ServerID, InstallerFolder);
            string versionPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleVersion);
            string hytaleInstallerZipPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleDownloaderZip);
            string hytaleInstallerPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleDownloader);
            string hytaleZipPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleZip);

            //Check JRE update 
            await DownloadCurrentJre();

            //Get Hytale Downlaoder
            if (!File.Exists(hytaleInstallerPath))
            {
                if (File.Exists(hytaleInstallerZipPath)) File.Delete(hytaleInstallerZipPath);
                if (!await DownloadFileAsync(DownloaderUrl, hytaleInstallerZipPath)) return null;
                ZipFile.ExtractToDirectory(new StreamReader(hytaleInstallerZipPath).BaseStream, ServerPath.GetServersServerFiles(serverData.ServerID, tmpInstallPath));
            }

            //update downloader
            Process update = StartProcessAsync(hytaleInstallerPath, $" -check-update");
            await update.WaitForExitAsync();

            string currentVersion = File.ReadAllText(versionPath);
            string remoteVersion = await GetRemoteBuild();

            if (currentVersion == remoteVersion)
                return null;

            File.Delete(hytaleZipPath);

            var downloaderProcess = StartProcessAsync(hytaleInstallerPath, $" -download-path {hytaleZipPath}");
            SendEnterPreventFreeze(downloaderProcess);
            await downloaderProcess.WaitForExitAsync();
            File.WriteAllText(versionPath, remoteVersion);

            return downloaderProcess;
        }

        public Process StartProcessAsync(string exe, string param = "")
        {
            Process p = null;
            try
            {
                p = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = ServerPath.GetServersServerFiles(serverData.ServerID),
                        FileName = exe,
                        Arguments = param,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                p.Start();

            }
            catch
            {
                Error = $"Could Not Execute ${exe}";
            }
            SendEnterPreventFreeze(p);
            return p;
        }

        public async Task<bool> DownloadFileAsync(string url, string relativePath)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync(url, relativePath);
                }
                return true;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return false;
            }

        }

        public string GetLocalBuild()
        {
            return File.ReadAllText(Functions.ServerPath.GetServersServerFiles(serverData.ServerID, HytaleVersion));
        }

        public async Task<string> GetRemoteBuild()
        {
            string hytaleInstallerPath = ServerPath.GetServersServerFiles(serverData.ServerID, HytaleDownloader);

            string remoteVersion = "";
            // --print-version => 2026.01.15-c04fdfe10
            Process version = StartProcessAsync(hytaleInstallerPath, $" --print-version");
            while (!version.StandardOutput.EndOfStream)
            {
                remoteVersion = version.StandardOutput.ReadLine();
                if (!string.IsNullOrEmpty(remoteVersion))
                    break;
            }

            await version.WaitForExitAsync();
            return remoteVersion;
        }

        public bool IsInstallValid()
        {
            //need to check for the hytale.zip as we can't extract it dueto the oauth
            string installPath = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, HytaleZip);
            Error = $"Fail to find {installPath}";
            return File.Exists(installPath);
        }

        public bool IsImportValid(string path)
        {
            string importPath = Path.Combine(path, StartPath);
            Error = $"Invalid Path! Fail to find {Path.GetFileName(StartPath)}";
            return File.Exists(importPath);
        }

        private async void SendEnterPreventFreeze(Process p)
        {
            try
            {
                await Task.Delay(300000);

                // Send enter 3 times per 3 seconds
                for (int i = 0; i < 3; i++)
                {
                    await Task.Delay(3000);

                    if (p == null || p.HasExited) { break; }
                    p.StandardInput.WriteLine(string.Empty);
                }

                // Wait 5 minutes
                await Task.Delay(300000);

                // Send enter 3 times per 3 seconds
                for (int i = 0; i < 3; i++)
                {
                    await Task.Delay(3000);

                    if (p == null || p.HasExited) { break; }
                    p.StandardInput.WriteLine(string.Empty);
                }
            }
            catch
            {

            }
        }

        private async Task DownloadCurrentJre()
        {
            string versionPath = ServerPath.GetServersServerFiles(serverData.ServerID, JreVersion);
            string jreZipPath = ServerPath.GetServersServerFiles(serverData.ServerID, JreZip);
            string jreDestPath = ServerPath.GetServersServerFiles(serverData.ServerID, JreRootPath);

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";
            string currentAPIVersion = "";

            if (File.Exists(versionPath))
            {
                currentAPIVersion = File.ReadAllText(versionPath);
            }

            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
            try
            {
                // Download the latest release information from the GitHub API
                string responseContent = webClient.DownloadString(JreApiUrl);
                JObject releaseInfo = JObject.Parse(responseContent);

                string version = (releaseInfo["name"].ToString()).Trim();
                if (version == currentAPIVersion)
                {
                    return;
                }
                //new JRE available, delete old and install new one
                ClearJre();
                // Get the download URL of the first asset (assuming there is at least one asset)
                var assets = releaseInfo["assets"].ToList();

                var winBinary = assets.Where(a => a["name"].ToString().Contains("OpenJDK25U-jre_x64_windows_hotspot_") && a["name"].ToString().EndsWith(".zip")).ToList();
                string downloadUrl = "";

                if (winBinary.Any())
                {
                    downloadUrl = (winBinary.First()["browser_download_url"].ToString()).Trim();
                    string[] urlSegments = downloadUrl.Split('/');
                    string filename = urlSegments[urlSegments.Length - 1];
                    string serverAPIFileName = filename;
                }

                await webClient.DownloadFileTaskAsync(new Uri(downloadUrl), jreZipPath);

                ZipFile.ExtractToDirectory(new StreamReader(jreZipPath).BaseStream, jreDestPath);

                File.WriteAllText(versionPath, version);
            }
            catch (WebException ex)
            {
                // Handle exceptions
                Error = $"Error: {ex.Message}";
            }

            return;
        }

        public void ClearJre()
        {
            string jreZipPath = Path.Combine(ServerPath.GetServersServerFiles(serverData.ServerID, InstallerFolder), "jreInstall.zip");
            string javaRoot = ServerPath.GetServersServerFiles(serverData.ServerID, JreRootPath);

            File.Delete(jreZipPath);
            System.IO.DirectoryInfo di = new DirectoryInfo(javaRoot);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
