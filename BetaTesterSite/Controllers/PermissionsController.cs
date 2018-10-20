using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BetaTesterSite.DAL;

namespace ACATWeb.Controllers
{
    //[Authorize]
    [Authorize("SystemVariablesManager")]
    public class PermissionsController : Controller
    {
        private readonly BetaTesterSite.DAL.BetaTesterContext context;

        public PermissionsController(BetaTesterSite.DAL.BetaTesterContext context)
        {
            this.context = context;
        }

        //[Authorize("SystemVariablesManager")]
        public IActionResult Index()
        {
            ViewData["Policies"] = context.Policy.ToList();
            ViewData["PolicyRoles"] = context.PolicyRole.ToList();
            ViewData["Roles"] = context.Role.ToList();
            return View();
        }

        [HttpPost]
        //[Authorize("SystemVariablesManager")]
        public IActionResult Manage(int[] adminPolicyId, int[] guestPolicyId)
        {
            var roles = context.Role.ToList();

            var adminRoleId = roles.Single(x => x.NormalizedName == "ADMINISTRATOR").Id;
            var gustRoleId = roles.Single(x => x.NormalizedName == "GUEST").Id;

            ClearPolicyRoles();

            foreach (var policyId in adminPolicyId)
                context.PolicyRole.Add(new PolicyRole { PolicyId = policyId, RoleId = adminRoleId });

            foreach (var policyId in guestPolicyId)
                context.PolicyRole.Add(new PolicyRole { PolicyId = policyId, RoleId = gustRoleId });
            this.context.SaveChanges();

            return Json(true);
        }

        public void ClearPolicyRoles()
        {
            var policyRoles = context.PolicyRole.ToArray();
            for (int i = 0; i < policyRoles.Length; i++)
            {
                context.PolicyRole.Remove(policyRoles[i]);
            }
            this.context.SaveChanges();
        }

    }
}