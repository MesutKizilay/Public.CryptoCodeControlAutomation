using Microsoft.AspNetCore.Mvc;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeStatusSummary;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetProducedCodeStatistics;
using CryptoCodeControlAutomation.Application.Features.PlannedOrders.Queries.GetListBySalesOrderItemId;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCodeStatusSummary(long? salesOrderItemId, long? plannedOrderId)
        {
            var query = new GetCodeStatusSummaryQuery
            {
                SalesOrderItemId = salesOrderItemId,
                PlannedOrderId = plannedOrderId
            };
            var result = await Mediator.Send(query);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlannedOrdersBySalesOrderItemId(long? salesOrderItemId)
        {
            var query = new GetListPlannedOrdersBySalesOrderItemIdQuery { SalesOrderItemId = salesOrderItemId };
            var items = await Mediator.Send(query);
            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducedCodeStatistics(string period, long? salesOrderItemId, long? plannedOrderId)
        {
            var query = new GetProducedCodeStatisticsQuery
            {
                Period = period,
                SalesOrderItemId = salesOrderItemId,
                PlannedOrderId = plannedOrderId
            };

            var items = await Mediator.Send(query);
            return Json(items);
        }
    }
}
