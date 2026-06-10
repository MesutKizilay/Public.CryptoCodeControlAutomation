using CryptoCodeControlAutomation.Application.Features.DataMatrixPrints.Commands.GeneratePdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    [Authorize(Policy = "AdminSupervisorOrOperator")]
    public class DataMatrixPrintsController : BaseController
    {
        public IActionResult DataMatrixPrints()
        {
            return View();
        }

        [HttpPost]
        [RequestSizeLimit(1000 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1000 * 1024 * 1024)]
        public async Task<IActionResult> GeneratePdf([FromForm] GenerateDataMatrixPdfCommand command)
        {
            var result = await Mediator.Send(command);
            return File(result.Content, result.ContentType, result.FileName);
        }
    }
}
