using Berk.Identity.Context;
using Berk.Identity.CustomDescriber;
using Berk.Identity.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Berk.Identity
{
	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddIdentity<AppUser, AppRole>(opt=> {
				opt.Password.RequireDigit = false; //say� i�ermesi gereken kod
				opt.Password.RequiredLength = 1; //bir karakter girmek
				opt.Password.RequireLowercase = false; //K���K HARF GEREklili�i
				opt.Password.RequireUppercase = false;
				opt.Password.RequireNonAlphanumeric = false; //alfabetik karakter zorunlulu�u
				opt.Lockout.MaxFailedAccessAttempts = 3; //3 yanl��tan sonra kilitlenecek
	            /*opt.SignIn.RequireConfirmedEmail = true;*/ //Databasedeki ilgili kayd�n e-mailconf. alan�na bakar, e�er false'sa is not allowed d�nd�r�r.
				//sen Identityerrordescriber� de�il de benim yazd���m t�rk�ele�tirilm	i� halini yolla diyoruz. entityframeworksstoresun hemen �n�ne ekliyoruz.

			}).AddErrorDescriber<CustomErrorDescriber>().AddEntityFrameworkStores<BerkContext>();

			services.ConfigureApplicationCookie(opt => 
			{
				opt.Cookie.HttpOnly = true; //cookie js ile �ekilemiyor
				opt.Cookie.SameSite = SameSiteMode.Strict; //ilgili domainde kullan�l�r
				opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; //always dersem sadece https'te �al��s�n, sameasrequest dersem http ile gelirse http, https ile ggelirse https ile �al��s�n
				opt.Cookie.Name = "BerkCookie";
				opt.ExpireTimeSpan = TimeSpan.FromDays(25); //25 g�n kullan�c� cookiesi hat�rlayacak
				opt.LoginPath = new PathString("/Home/SignIn"); //yetkisi olmay�p giri� yapmam�� bir kullan�c� bir alana girmeye �al��t���nda Logine yolluyordu. //diyelim Home/AdminPanel ayzd�k hata ��kt�(login hatas�) onu de�i�tirmeye �al��aca��m.PathString ile kendi yolumuzu belirtebiliriz
				opt.AccessDeniedPath = new PathString("/Home/AccessDenied"); //Member olarak giri� yapm��t�m.Userdaki role authorizesine indexe ula�mak isterken kar��ma Account/AccesDaniedPath hatas� ��kt� ve d�zenledim.





			});


			services.AddDbContext<BerkContext>(opt=> {
				opt.UseSqlServer("server=DESKTOP-LCS594S\\SQLSERVER2019EXP; database=IdentityDB; integrated security=true;");
			
			});
			services.AddControllersWithViews();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseStaticFiles(); //wwwroot
			app.UseStaticFiles(new StaticFileOptions {
				RequestPath = "/node_modules",
				FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "node_modules"))

			});


			app.UseRouting();
			//routingin alt�nda endpointin �st�ne yani buraya ilk Authentication sonra authorization olmak zorunda
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}
