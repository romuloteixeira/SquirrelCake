using System.Diagnostics;
using System.Linq;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SquirrelCake.Application.Pages
{
    public class UpdateModel : PageModel
    {
        public int count = 0;
        public string mensage = "Início";
        public string appVersion;

        public void OnGet()
        {
            //var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            //appVersion = fileVersionInfo.FileVersion;
            appVersion = "5.22.15";
        }

        public void OnPost()
        {
            if (HybridSupport.IsElectronActive)
            {
                count++;
            var autoUpdater = Electron.AutoUpdater;
                Electron.AutoUpdater.OnError += (message) => Electron.Dialog.ShowErrorBox("Error", message);
                Electron.AutoUpdater.OnCheckingForUpdate += async () => await Electron.Dialog.ShowMessageBoxAsync("Checking for Update");
                Electron.AutoUpdater.OnUpdateNotAvailable += async (info) => await Electron.Dialog.ShowMessageBoxAsync("Update not available");
                Electron.AutoUpdater.OnUpdateAvailable += async (info) => await Electron.Dialog.ShowMessageBoxAsync("Update available" + info.Version);
            
            mensage = "passou";

                Electron.AutoUpdater.OnDownloadProgress += (info) =>
                {
                    var message1 = "Download speed: " + info.BytesPerSecond + "\n<br/>";
                    var message2 = "Downloaded " + info.Percent + "%" + "\n<br/>";
                    var message3 = $"({info.Transferred}/{info.Total})" + "\n<br/>";
                    var message4 = "Progress: " + info.Progress + "\n<br/>";
                    var information = message1 + message2 + message3 + message4;
                    
                    mensage = information;

                    var mainWindow = Electron.WindowManager.BrowserWindows.First();
                    Electron.IpcMain.Send(mainWindow, "auto-update-reply", information);
                };
                Electron.AutoUpdater.OnUpdateDownloaded += async (info) => await Electron.Dialog.ShowMessageBoxAsync("Update complete!" + info.Version);

                Electron.IpcMain.On("auto-update", async (args) =>
                {
                    // Electron.NET CLI Command for deploy:
                    // electronize build /target win /electron-params --publish=always

                    var currentVersion = await Electron.App.GetVersionAsync();
                    var updateCheckResult = await Electron.AutoUpdater.CheckForUpdatesAndNotifyAsync();
                    var availableVersion = updateCheckResult.UpdateInfo.Version;
                    string information = $"Current version: {currentVersion} - available version: {availableVersion}";

                    mensage = $"updateCheckResult: {updateCheckResult} - currentVersion: {currentVersion} - availableVersion: {availableVersion}";

                    var mainWindow = Electron.WindowManager.BrowserWindows.First();
                    Electron.IpcMain.Send(mainWindow, "auto-update-reply", information);
                });
            mensage = "terminou";
            }

            //autoUpdater.

            Electron.AutoUpdater.CheckForUpdatesAndNotifyAsync();
        }
    }
}
