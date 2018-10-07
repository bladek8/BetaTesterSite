using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetaTesterSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BetaTesterSite.Controllers
{
    public class UserController : Controller
    {
        private readonly SignInManager<DAL.Identity.User> _signInManager;
        private readonly UserManager<DAL.Identity.User> userManager;
        private readonly DAL.BetaTesterContext context;

        public UserController(DAL.BetaTesterContext context, UserManager<DAL.Identity.User> userManager, SignInManager<DAL.Identity.User> signInManager)
        {
            this.userManager = userManager;
            this.context = context;
            this._signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Manage")]
        public async Task<IActionResult> _Manage(UserViewModel model)
        {
            var a = (from u in context.User select u);
            var userId = Create(model);

            return await Task.Run<IActionResult>(() => Json(userId));
        }

        public int Create(UserViewModel model)
        {
            return _Create(model).Result;
        }

        public async Task<int> _Create(UserViewModel model)
        {
            var user = new DAL.Identity.User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive
            };

            var identityResult = await this.userManager.CreateAsync(user, model.Password);
            if (identityResult.Succeeded)
                return user.Id;
            else
                return 0;
                //throw new Exception(string.Join(';', identityResult.Errors.Select(x => x.Code)));
        }


        [AllowAnonymous]
        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> _Login(Models.LoginViewModel model, string returnUrl = null)
        {
            var result = await this._signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
                return await Task.Run<IActionResult>(() => Json(true));
            else
                return await Task.Run<IActionResult>(() => Json(false));
        }

        public async Task<IActionResult> LogOut()
        {
            await this._signInManager.SignOutAsync();
            return await Task.Run<IActionResult>(() => RedirectToAction("Index", "Home"));
        }

        [HttpPost]
        [ActionName("List")]
        public async Task<IActionResult> _List(BetaTesterSite.Models.Shared.DataTablesAjaxPostModel filter)
        {
            var d = (from y in context.User where y.IsDeleted == false select y);

            return await Task.Run<IActionResult>(() => Json(new
            {
                draw = filter.draw,
                recordsTotal = d.Count(),
                recordsFiltered = d.Count(),
                data = GetUserViewModels(d)
            }));
        }

        List<UserViewModel> GetUserViewModels(IQueryable<DAL.Identity.User> data)
        {
            return (from y in data
                    select new BetaTesterSite.Models.UserViewModel()
                    {
                        UserId = y.Id,
                        FirstName = y.FirstName,
                        LastName = y.LastName,
                        Email = y.Email,
                        PhoneNumber = y.PhoneNumber,
                        IsActive = y.IsActive
                    }).ToList();
        }

        [HttpPost]
        [ActionName("RemoveUser")]
        public async Task<IActionResult> RemoveUser(int id)
        {
            if (!context.User.Any(x => x.Id == id)) return Json(false);

            var user = context.User.Single(x => x.Id == id);
            user.IsDeleted = true;

            context.User.Update(user);
            context.SaveChanges();

            return await Task.Run<IActionResult>(() => Json(true));
        }
    }
}