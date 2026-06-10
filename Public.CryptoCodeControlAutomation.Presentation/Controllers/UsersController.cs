using Core.Application.Request;
using CryptoCodeControlAutomation.Application.Features.Users.Commands.Create;
using CryptoCodeControlAutomation.Application.Features.Users.Commands.Delete;
using CryptoCodeControlAutomation.Application.Features.Users.Commands.Update;
using CryptoCodeControlAutomation.Application.Features.Users.Queries.GetList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    public class UsersController : BaseController
    {    
        //[EnableRateLimiting("fixed-by-user")]
        public IActionResult Users()
        {
            //ViewBag.Claim = User.FindFirst(ClaimTypes.Role)?.Value;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetList(PageRequest pageRequest, bool withDeleted)
        {
            GetListUserQuery getListUserQuery = new GetListUserQuery()
            {
                PageRequest = pageRequest,
                WithDeleted = withDeleted
            };
            var users = await Mediator.Send(getListUserQuery);
            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateUserCommand updateUserCommand)
        {
            await Mediator.Send(updateUserCommand);
            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserCommand createUserCommand)
        {
            await Mediator.Send(createUserCommand);
            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            DeleteUserCommand deleteUserCommand = new DeleteUserCommand() { Id = id };

            await Mediator.Send(deleteUserCommand);
            return Json(true);
        }
    }
}
