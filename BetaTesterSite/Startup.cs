using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BetaTesterSite
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DAL.BetaTesterContext>(options => options.UseSqlite(connection));
            services.AddDbContext<DAL.Identity.ApplicationDbContext>(options => options.UseSqlite(connection));

            services.AddIdentity<DAL.Identity.User, DAL.Identity.Role>()
                .AddEntityFrameworkStores<DAL.Identity.ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            var sp = services.BuildServiceProvider();
            var context = sp.GetService<DAL.BetaTesterContext>();

            services.AddAuthorization(options =>
            {
                var roles = context.Role.ToArray();
                var policyRole = context.PolicyRole.ToArray();
                foreach (var policy in context.Policy)
                {
                    var roleIds = policyRole.Where(x => x.PolicyId == policy.PolicyId).Select(x => x.RoleId);
                    var _roles = new List<string>();
                    foreach (var roleId in roleIds)
                    {
                        var role = roles.Where(x => x.Id == roleId);
                        if (role.Count() > 0)
                            _roles.Add(role.First().Name);
                    }

                    if (roles.Count() > 0 && _roles.Count > 0)
                        options.AddPolicy(policy.Name, pol => pol.RequireRole(_roles));
                    else
                        options.AddPolicy(policy.Name, pol => pol.RequireRole(roles.Select(X => X.Name)));
                    //options.AddPolicy(policy.Name, pol => pol.RequireRole());
                }
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                "Default",
                "{controller}/{action}",
                new { controller = "Home", action = "Index" });
            });

            CreateRoles(services).Wait();
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<DAL.Identity.Role>>();

            Microsoft.AspNetCore.Identity.IdentityResult roleResult;

            var roles = new List<string> {
                "Administrator",
                "Guest"
            };

            foreach (var role in roles)
            {
                var roleCheck = await roleManager.RoleExistsAsync(role);
                if (!roleCheck)
                    roleResult = await roleManager.CreateAsync(new DAL.Identity.Role() { Name = role });
            }
        }
    }
}
