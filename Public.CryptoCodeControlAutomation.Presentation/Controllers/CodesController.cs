using Core.Application.Request;
using CryptoCodeControlAutomation.Application.Features.Codes.Commands.RecoverCodes;
using CryptoCodeControlAutomation.Application.Features.Codes.Commands.ScrapCodes;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.ExportCodeReport;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeReportList;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetListCodesByPlannedOrderId;
using CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetPlannedOrderByPalletNumber;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    [Authorize(Policy = "AdminSupervisorOrOperator")]
    public class CodesController : BaseController 
    {
        public IActionResult Scraps()
        {
            return View();
        }

        public IActionResult Recover()
        {
            return View();
        }

        public IActionResult CodeReports()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetPlannedOrderByPalletNumber([FromBody] GetPlannedOrderByPalletNumberQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetListCodesByPlannedOrderId(string plannedOrderNo)
        {
            var query = new GetListCodesByPlannedOrderIdQuery { PlannedOrderNo = plannedOrderNo };
            var items = await Mediator.Send(query);
            return Json(items);
        }

        [HttpPost]
        public async Task<IActionResult> ScrapCodes([FromBody] ScrapCodesCommand scrapCodesCommand)
        {
            var result = await Mediator.Send(scrapCodesCommand);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> RecoverCodes([FromBody] RecoverCodesCommand recoverCodesCommand)
        {
            var result = await Mediator.Send(recoverCodesCommand);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetCodeReportList(PageRequest pageRequest, [FromBody] GetCodeReportListQuery query)
        {
            query.PageRequest = pageRequest;
            var result = await Mediator.Send(query);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ExportCodeReport([FromBody] ExportCodeReportQuery query)
        {
            var result = await Mediator.Send(query);
            return File(result.Content, result.ContentType, result.FileName);
        }
    }
}
