using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BetaTesterSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<DAL.Identity.User> _signInManager;
        private readonly UserManager<DAL.Identity.User> userManager;
        private readonly DAL.BetaTesterContext context;

        public AccountController(DAL.BetaTesterContext context, UserManager<DAL.Identity.User> userManager, SignInManager<DAL.Identity.User> signInManager)
        {
            this.userManager = userManager;
            this.context = context;
            this._signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ActionName("Info")]
        public async Task<IActionResult> _Info([FromForm]BetaTesterSite.Models.NewPasswordViewModel model)
        {
            var user = await this.userManager.GetUserAsync(User);
            bool passwordOk = await this.userManager.CheckPasswordAsync(user, model.CurrentPassword);

            var data = await userManager.GetUserAsync(User);
            var _model = GetUserViewModels(data);

            if (!passwordOk)
            {
                ViewData["ErrorMessage"] = "Senha atual incorreta.";
                return View(_model);
            }
            else
            {
                var result = await this.userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                return RedirectToAction("Info", new { saved = true });
            }
        }

        public async Task<IActionResult> Info()
        {
            var data = await userManager.GetUserAsync(User);
            var model = GetUserViewModels(data);

            return View(model);
        }


        Models.UserViewModel GetUserViewModels(DAL.Identity.User data)
        {
            return new BetaTesterSite.Models.UserViewModel()
            {
                UserId = data.Id,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                PhoneNumber = data.PhoneNumber,
                IsActive = data.IsActive
            };
        }
    }
}