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
				opt.Password.RequireDigit = false; //sayý içermesi gereken kod
				opt.Password.RequiredLength = 1; //bir karakter girmek
				opt.Password.RequireLowercase = false; //KÜÇÜK HARF GEREkliliði
				opt.Password.RequireUppercase = false;
				opt.Password.RequireNonAlphanumeric = false; //alfabetik karakter zorunluluðu
				opt.Lockout.MaxFailedAccessAttempts = 3; //3 yanlýþtan sonra kilitlenecek
	            /*opt.SignIn.RequireConfirmedEmail = true;*/ //Databasedeki ilgili kaydýn e-mailconf. alanýna bakar, eðer false'sa is not allowed döndürür.
				//sen Identityerrordescriberý deðil de benim yazdýðým türkçeleþtirilm	iþ halini yolla diyoruz. entityframeworksstoresun hemen önüne ekliyoruz.

			}).AddErrorDescriber<CustomErrorDescriber>().AddEntityFrameworkStores<BerkContext>();

			services.ConfigureApplicationCookie(opt => 
			{
				opt.Cookie.HttpOnly = true; //cookie js ile çekilemiyor
				opt.Cookie.SameSite = SameSiteMode.Strict; //ilgili domainde kullanýlýr
				opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; //always dersem sadece https'te çalýþsýn, sameasrequest dersem http ile gelirse http, https ile ggelirse https ile çalýþsýn
				opt.Cookie.Name = "BerkCookie";
				opt.ExpireTimeSpan = TimeSpan.FromDays(25); //25 gün kullanýcý cookiesi hatýrlayacak
				opt.LoginPath = new PathString("/Home/SignIn"); //yetkisi olmayýp giriþ yapmamýþ bir kullanýcý bir alana girmeye çalýþtýðýnda Logine yolluyordu. //diyelim Home/AdminPanel ayzdýk hata çýktý(login hatasý) onu deðiþtirmeye çalýþacaðým.PathString ile kendi yolumuzu belirtebiliriz
				opt.AccessDeniedPath = new PathString("/Home/AccessDenied"); //Member olarak giriþ yapmýþtým.Userdaki role authorizesine indexe ulaþmak isterken karþýma Account/AccesDaniedPath hatasý çýktý ve düzenledim.





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
			//routingin altýnda endpointin üstüne yani buraya ilk Authentication sonra authorization olmak zorunda
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}
