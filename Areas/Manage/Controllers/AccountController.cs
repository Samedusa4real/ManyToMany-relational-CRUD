using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PustokTemplate.Models;

namespace PustokTemplate.Areas.Manage.Controllers
{
    [Area("manage")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public AccountController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> CreateAdmin()
        {
            AppUser user = new AppUser
            {
                UserName = "admin",
                IsAdmin = true,
            };

            var result = await _userManager.CreateAsync(user,"Admin123");

            return Json(result);
        }

        public IActionResult Login()
        {
            return View();
        }


    }
}
