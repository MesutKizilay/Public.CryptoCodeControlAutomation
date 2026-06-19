using Core.Application.Request;
using CryptoCodeControlAutomation.Application.Features.Codes.Commands.RecoverCodes;
using CryptoCodeControlAutomation.Application.Features.Codes.Commands.ScrapCodes;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.ExportCodeReport;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeLookup;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeReportList;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetListCodesByPlannedOrderId;
using CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetPlannedOrderByPalletNumber;
using CryptoCodeControlAutomation.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text.Json;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    [Authorize(Policy = "AdminSupervisorOrOperator")]
    public class CodesController : BaseController 
    {
        private const int RecoverScannerPort = 4997;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly MoxaTcpDemoService _moxaTcpDemoService;

        public CodesController(MoxaTcpDemoService moxaTcpDemoService)
        {
            _moxaTcpDemoService = moxaTcpDemoService;
        }

        public IActionResult Scraps()
        {
            return View();
        }

        public IActionResult Recover()
        {
            return View();
        }

        public IActionResult CodeLookup()
        {
            return View();
        }

        [HttpPost]
        public Task<IActionResult> StartRecoverScanner()
        {
            return StartScanner();
        }

        [HttpPost]
        public Task<IActionResult> StartScrapsScanner()
        {
            return StartScanner();
        }

        [HttpPost]
        public Task<IActionResult> StartCodeLookupScanner()
        {
            return StartScanner();
        }

        private async Task<IActionResult> StartScanner()
        {
            try
            {
                await _moxaTcpDemoService.StartAsync(RecoverScannerPort);
                return Ok(new { port = RecoverScannerPort });
            }
            catch (SocketException exception)
            {
                return BadRequest(new { message = $"4997 portu dinlenemedi: {exception.Message}" });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpGet]
        public Task RecoverScannerStream(CancellationToken cancellationToken)
        {
            return StreamScanner(cancellationToken);
        }

        [HttpGet]
        public Task ScrapsScannerStream(CancellationToken cancellationToken)
        {
            return StreamScanner(cancellationToken);
        }

        [HttpGet]
        public Task CodeLookupScannerStream(CancellationToken cancellationToken)
        {
            return StreamScanner(cancellationToken);
        }

        private async Task StreamScanner(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Append("X-Accel-Buffering", "no");

            var (subscriptionId, reader) = _moxaTcpDemoService.Subscribe();

            try
            {
                await Response.WriteAsync(": connected\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                await foreach (var message in reader.ReadAllAsync(cancellationToken))
                {
                    var json = JsonSerializer.Serialize(message, JsonOptions);
                    await Response.WriteAsync($"event: code\ndata: {json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _moxaTcpDemoService.Unsubscribe(subscriptionId);
            }
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
        public async Task<IActionResult> GetCodeLookup([FromBody] GetCodeLookupQuery query)
        {
            var result = await Mediator.Send(query);
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
