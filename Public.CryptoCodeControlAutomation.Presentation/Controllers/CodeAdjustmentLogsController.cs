using Core.Application.Request;
using Core.Persistence.Dynamic;
using CryptoCodeControlAutomation.Application.Features.CodeAdjustmentLogs.Queries.GetList;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    public class CodeAdjustmentLogsController : BaseController
    {
        public IActionResult CodeAdjustmentLogs()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetList(PageRequest pageRequest, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            var query = new GetListCodeAdjustmentLogWithPaginateQuery
            {
                PageRequest = pageRequest,
                DynamicQuery = dynamicQuery
            };

            var result = await Mediator!.Send(query);
            return Json(result);
        }
    }
}
