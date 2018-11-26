using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace dex_webapp.Controllers
{
    [Route("api/appsettings")]
    public class AppsettingsController : Controller
    {
        private readonly IConfiguration _config;

        public AppsettingsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("etherdeltaaddress")]
        public IActionResult GetCrowdsaleAddress()
        {
            return Json(new { Result = _config["EthereumSettings:EtherDeltaContractAddress"] });
        }
    }
}
