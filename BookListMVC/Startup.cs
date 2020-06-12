using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookListMVC.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore.Relational;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookListMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            Console.WriteLine("Using Development: " + env.IsDevelopment);

            /** Use PostgreSQL for heroku deployment, otherwise use local SQL Server instance */
            if (env.IsDevelopment) {
                services.AddDbContext<ApplicationDBContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                );
            }
            else {

                // Get Environment variable from Heroku
                string _pgConnString = Environment.GetEnvironmentVariable("DATABASE_URL");
                _pgConnString.Replace("//", "");

                char[] delimiters = {":", "/", "@", "?"};
                string[] connStringArr = _pgConnString.Split(delimiters);

                connStringArr = connStringArr.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                _pgBuiltConnStr = 
                "host=" + connStringArr[3] +
                ";port=" + connStringArr[4] + 
                ";database=" + connStringArr[5] +
                ";uid=" + connStringArr[1] + 
                ";pwd=" + connStringArr[2] + "TrustServerCertificate=true";


                services.AddDbContext<ApplicationDBContext>(
                //options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                options => options.UseNpgsql(_pgBuiltConnStr)
                );
            }

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
