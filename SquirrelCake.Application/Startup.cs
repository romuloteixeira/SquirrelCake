using ElectronNET.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace SquirrelCake.Application
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            Task.Run(async () => await Electron.WindowManager.CreateWindowAsync());

            if (HybridSupport.IsElectronActive)
            {
                CreateWindow();
            }

            ConfigureAutoUpdater();
        }

        private static void ConfigureAutoUpdater()
        {
            if (HybridSupport.IsElectronActive)
            {
                var autoUpdater = Electron.AutoUpdater;
                Electron.AutoUpdater.OnError += (message) => Electron.Dialog.ShowErrorBox("Error", message);
                Electron.AutoUpdater.OnCheckingForUpdate += async () => await Electron.Dialog.ShowMessageBoxAsync("Checking for Update");
                Electron.AutoUpdater.OnUpdateNotAvailable += async (info) => await Electron.Dialog.ShowMessageBoxAsync("Update not available");
                Electron.AutoUpdater.OnUpdateAvailable += async (info) => await Electron.Dialog.ShowMessageBoxAsync("Update available" + info.Version);


                Electron.AutoUpdater.OnDownloadProgress += (info) =>
                {
                    var message1 = "Download speed: " + info.BytesPerSecond + "\n<br/>";
                    var message2 = "Downloaded " + info.Percent + "%" + "\n<br/>";
                    var message3 = $"({info.Transferred}/{info.Total})" + "\n<br/>";
                    var message4 = "Progress: " + info.Progress + "\n<br/>";
                    var information = message1 + message2 + message3 + message4;

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

                    var mainWindow = Electron.WindowManager.BrowserWindows.First();
                    Electron.IpcMain.Send(mainWindow, "auto-update-reply", information);
                });
            }
        }

        private async void CreateWindow()
        {
            var window = await Electron.WindowManager.CreateWindowAsync();
            window.OnClosed += () => {
                Electron.App.Quit();
            };
        }
    }
}
