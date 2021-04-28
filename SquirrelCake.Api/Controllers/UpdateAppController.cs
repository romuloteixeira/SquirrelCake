using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SquirrelCake.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UpdateAppController : ControllerBase
    {
        [HttpGet]
        public UpdateResponse Update()
        {
            return new UpdateResponse
            {
                Url = "http://teste.com/download/version/1.0.2/windows_32?filetype=zip",
            };
        }
    }
}
