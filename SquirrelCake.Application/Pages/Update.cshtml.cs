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
            var currentVersion = Electron.App.GetVersionAsync().Result;
            appVersion = currentVersion;
            //var semVer = Electron.AutoUpdater.CurrentVersionAsync.Result;
            //appVersion = Electron..GetVersionAsync().Result;
        }

        public void OnPost()
        {
            count++;
            mensage = "passou";

            Electron.AutoUpdater.CheckForUpdatesAndNotifyAsync();
            
            mensage = "terminou";
        }
    }
}
