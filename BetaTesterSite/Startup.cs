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
        //    public Startup(IConfiguration configuration)
        //    {
        //        Configuration = configuration;
        //    }

        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            //var builder = new ConfigurationBuilder()
            //.AddJsonFile("appsettings.json")
            //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            //using (var db = new DAL.BetaTesterContext())
            //{
            //    db.Database.EnsureCreated();
            //    db.Database.Migrate();
            //}
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")))
            //services.AddEntityFrameworkSqlite().AddDbContext<DAL.BetaTesterContext>();
            //services.AddEntityFrameworkSqlite().AddDbContext<DAL.Identity.ApplicationDbContext>();
            services.AddDbContext<DAL.BetaTesterContext>(options => options.UseSqlite(connection));
            services.AddDbContext<DAL.Identity.ApplicationDbContext>(options => options.UseSqlite(connection));

            services.AddIdentity<DAL.Identity.User, DAL.Identity.Role>()
                .AddEntityFrameworkStores<DAL.Identity.ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();
            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
        }
    }
}
