using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SquirrelCake.Application.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            if (Electron.App.CommandLine.HasSwitchAsync("user").Result)
            {
                string value = Electron.App.CommandLine.GetSwitchValueAsync("user").Result;
                Electron.Dialog.ShowMessageBoxAsync($"User: {value}").Wait();
            }
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
