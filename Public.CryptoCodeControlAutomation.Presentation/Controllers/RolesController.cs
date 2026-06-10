using Microsoft.AspNetCore.Mvc;
using CryptoCodeControlAutomation.Application.Features.Roles.Queries.GetList;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    public class RolesController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            GetListRoleQuery getListRoleQuery= new GetListRoleQuery() { };
            var users = await Mediator.Send(getListRoleQuery);
            return Json(users);
        }
    }

}

