using Core.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CryptoCodeControlAutomation.Application.Features.Auth.Commands.Login;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        public IActionResult Login(string? returnUrl)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            LoginCommand loginCommand = new() { UserForLoginDto = userForLoginDto };
            LoggedResponse result = await Mediator.Send(loginCommand);

            Response.Cookies.Append("AccessToken", result.AccessToken!.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict, // veya Strict
                //Expires = DateTimeOffset.UtcNow.AddSeconds(5),
                Expires = result.AccessToken.Expiration,
                IsEssential = true,
            });

            return Json(result);

            //return Ok(result);
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login", "Auth");
        }
    }
}
