using CryptoCodeControlAutomation.Application.Features.UploadJobs.Queries.GetBySalesOrderItemId;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    public class UploadJobsController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetBySalesOrderItemId(long id)
        {
            var query = new GetUploadJobsBySalesOrderItemIdQuery { SalesOrderItemId = id };
            var items = await Mediator.Send(query);
            return Json(items);
        }
    }
}
