using Core.Application.Request;
using Core.Persistence.Dynamic;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Create;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Delete;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Update;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.UpdateApprovalStatus;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.UpdateStatus;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetById;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetList;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetNextSalesOrderNo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    //[Authorize(Roles ="asd")]
    public class SalesOrderItemsController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SalesOrderItemsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _environment = environment;
        }

        public IActionResult SalesOrderItems()
        {
            return View();
        }

        public IActionResult Approvals()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            GetListSalesOrderItemQuery query = new GetListSalesOrderItemQuery();
            var items = await Mediator.Send(query);
            return Json(items);
        }

        [HttpPost]
        public async Task<IActionResult> GetList(PageRequest pageRequest, bool withDeleted, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            GetListSalesOrderItemWithPaginateQuery getListSalesOrderItemWithPaginateQuery = new GetListSalesOrderItemWithPaginateQuery()
            {
                PageRequest = pageRequest,
                DynamicQuery = dynamicQuery,
                WithDeleted = withDeleted
            };

            var items = await Mediator.Send(getListSalesOrderItemWithPaginateQuery);
            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(long id)
        {
            GetByIdSalesOrderItemQuery query = new GetByIdSalesOrderItemQuery() { Id = id };
            var item = await Mediator.Send(query);
            return Json(item);
        }

        [HttpGet]
        public async Task<IActionResult> GetNextSalesOrderNo()
        {
            GetNextSalesOrderNoQuery query = new GetNextSalesOrderNoQuery();
            var nextSalesOrderNo = await Mediator.Send(query);
            return Json(new { salesOrderNo = nextSalesOrderNo });
        }

        [HttpPost]
        [RequestSizeLimit(1000 * 1024 * 1024)] // 100 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 1000 * 1024 * 1024)]
        public async Task<IActionResult> Create([FromForm] CreateSalesOrderItemCommand createSalesOrderItemCommand)
        {
            createSalesOrderItemCommand.UploadsBasePath = Path.Combine(_environment.ContentRootPath);
            var result = await Mediator.Send(createSalesOrderItemCommand);
            return Json(new { success = true, salesOrderItemId = result.SalesOrderItemId, uploadJobId = result.UploadJobId });
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateSalesOrderItemCommand updateSalesOrderItemCommand)
        {
            await Mediator.Send(updateSalesOrderItemCommand);
            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApprovalStatus([FromBody] UpdateSalesOrderItemApprovalStatusCommand command)
        {
            await Mediator.Send(command);
            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(UpdateSalesOrderItemStatusCommand command)
        {
            await Mediator.Send(command);
            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            DeleteSalesOrderItemCommand deleteSalesOrderItemCommand = new DeleteSalesOrderItemCommand() { Id = id };
            await Mediator.Send(deleteSalesOrderItemCommand);
            return Json(true);
        }
    }
}
