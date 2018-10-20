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
        [Authorize(Policy = "UserManager")]
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Policy = "UserManager")]
        public IActionResult Manage(int? id)
        {
            var user = new Models.UserViewModel();
            ViewData["Roles"] = context.Role.ToList();
            if (id.HasValue)
                user = GetUserViewModels(context.User.SingleOrDefault(x => x.Id == id));
            return View(user);
        }

        [HttpPost]
        [ActionName("Manage")]
        [AllowAnonymous]
        public async Task<IActionResult> _Manage(UserViewModel model)
        {
            int userId;
            if (model.UserId.HasValue)
            {
                Update(model);
                userId = model.UserId.Value;
            }
            else
                userId = Create(model);


            ClearUserRoles(userId);
            AddUserToRole(userId, (string.IsNullOrWhiteSpace(model.Role) ? "Administrator" : model.Role));

            return await Task.Run<IActionResult>(() => Json(userId));
        }

        [AllowAnonymous]
        public int Create(UserViewModel model)
        {
            var id = _Create(model).Result;
            AddUserToRole(id, "Administrator");

            return id;
        }

        [AllowAnonymous]
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

        [HttpPost]
        [ActionName("List")]
        public IActionResult _List(BetaTesterSite.Models.Shared.DataTablesAjaxPostModel filter)
        {
            var d = (from y in context.User where y.IsDeleted == false select y);
            var _d = GetUserViewModels(d);
            return Json(new
            {
                draw = filter.draw,
                recordsTotal = d.Count(),
                recordsFiltered = d.Count(),
                data = _d
            });
        }

        [Authorize(Policy = "UserManager")]
        List<UserViewModel> GetUserViewModels(IQueryable<DAL.Identity.User> data)
        {
            return (from y in data
                    join ur in context.AspNetUserRoles on y.Id equals ur.UserId
                    join ro in context.Role on ur.RoleId equals ro.Id
                    select new BetaTesterSite.Models.UserViewModel()
                    {
                        UserId = y.Id,
                        FirstName = y.FirstName,
                        LastName = y.LastName,
                        Email = y.Email,
                        PhoneNumber = y.PhoneNumber,
                        IsActive = y.IsActive,
                        Role = ro.Description
                    }).ToList();
        }

        [Authorize(Policy = "UserManager")]
        UserViewModel GetUserViewModels(DAL.Identity.User data)
        {
            return new BetaTesterSite.Models.UserViewModel()
            {
                UserId = data.Id,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                PhoneNumber = data.PhoneNumber,
                IsActive = data.IsActive,
                Role = GetRoleByUserId(data.Id)
            };
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

        [Authorize(Policy = "UserManager")]
        public void Update(UserViewModel model)
        {
            var user = context.User.Single(x => x.Id == model.UserId);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.PasswordHash = this.userManager.PasswordHasher.HashPassword(user, model.Password);

            this.userManager.UpdateAsync(user);

            this.context.User.Update(user);
            this.context.SaveChanges();
        }


        [Authorize(Policy = "UserManager")]
        public void ClearUserRoles(int userId)
        {
            var userRoles = this.context.AspNetUserRoles.Where(x => x.UserId.Equals(userId)).ToArray();
            for (int i = userRoles.Length - 1; i == 0; i--)
            {
                this.context.AspNetUserRoles.Remove(userRoles[i]);
            }
            this.context.SaveChanges();
        }

        [AllowAnonymous]
        public void AddUserToRole(int userId, string role)
        {
            var r = this.context.Role.Single(x => x.NormalizedName.Equals(role.ToUpper()));
            var anur = new DAL.Identity.AspNetUserRoles()
            {
                RoleId = r.Id,
                UserId = userId
            };

            this.context.Add(anur);
            this.context.SaveChanges();
        }

        [Authorize(Policy = "UserManager")]
        public string GetRoleByUserId(int id)
        {
            var r = (from y in context.User
                     join ur in context.AspNetUserRoles on y.Id equals ur.UserId
                     join ro in context.Role on ur.RoleId equals ro.Id
                     where y.Id == id
                     select ro.Description);

            return r.Count() > 0 ? r.First() : null;
            //return  r.Count() > 0 ? r.First() : null;
        }
    }
}