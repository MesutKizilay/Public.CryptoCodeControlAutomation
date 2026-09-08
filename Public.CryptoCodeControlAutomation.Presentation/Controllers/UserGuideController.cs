using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    [Authorize(Policy = "AdminSupervisorOrOperator")]
    public class UserGuideController : Controller
    {
        private const string FileName = "Kripto_Kod_Kontrol_Otomasyon_Sistemi_Yetkili_Kullanici_Kilavuzu.pdf";
        private readonly IWebHostEnvironment _environment;

        public UserGuideController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            string filePath = Path.Combine(_environment.WebRootPath, "documents", FileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            return new PhysicalFileResult(filePath, "application/pdf")
            {
                EnableRangeProcessing = true
            };
        }
    }
}
