using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookListMVC.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookListMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment _env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("Using Development: " + _env.IsDevelopment());

            /** Use PostgreSQL for heroku deployment, otherwise use local SQL Server instance */
            if (_env.IsDevelopment()) {
                services.AddDbContext<ApplicationDBContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                );
            }
            else {
                
                // Get Environment variable from Heroku
                // Example Heroku PostgreSQL URL below
                //string _pgConnString = "postgres://zjqjovbyk:b3f6e10aa4fe6c06a7b595e7a38e802c02da934f482d43b17dab1d@ec2-52-72.compute-1.amazonaws.com:5432/dft2vrp6ot7ef";
                string _pgConnString = Environment.GetEnvironmentVariable("DATABASE_URL");
                _pgConnString.Replace("//", "");

                char[] delimiters = {':', '/', '@', '?'};
                string[] connStringArr = _pgConnString.Split(delimiters);

                connStringArr = connStringArr.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                string _pgBuiltConnStr = 
                "host=" + connStringArr[3] +
                ";port=" + connStringArr[4] + 
                ";database=" + connStringArr[5] +
                ";uid=" + connStringArr[1] + 
                ";pwd=" + connStringArr[2] + ";sslmode=Require;TrustServerCertificate=true";


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
