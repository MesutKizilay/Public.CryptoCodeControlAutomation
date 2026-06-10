using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CryptoCodeControlAutomation.Presentation.Models;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    //[AllowAnonymous]
    [Authorize(Policy = "AdminSupervisorOrOperator")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Homepage()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult NotFound404()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
