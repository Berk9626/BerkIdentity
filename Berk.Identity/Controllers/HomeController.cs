using Berk.Identity.Entities;
using Berk.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

//usermanager,rolemanager,signinmanager--kullanıcı kayıtı için UserManager
namespace Berk.Identity.Controllers
{
	[AutoValidateAntiforgeryToken] //dışardan clienta gelecek sahte bildirimleri engeller. Bu sunucunun üretmediği belli bir x noktadan istekler yapılamaz.
	public class HomeController : Controller
	{
		private readonly UserManager<AppUser> _userManager; //userManager AppUser için çalıştırdım.
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<AppRole> _roleManager;


		public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		public IActionResult Index()
		{

			return View();
		}

		public IActionResult Create()
		{

			return View(new UserCreateModel());
		}
		[HttpPost]
		public async Task<IActionResult> Create(UserCreateModel model)
		{
			if (ModelState.IsValid)
			{
				AppUser user = new AppUser()
				{
					UserName = model.UserName,
					Gender = model.Gender,
					Email = model.Email,

				};


				var identityResult = await _userManager.CreateAsync(user, model.Password); //vermiş olduğum usr ve parola ile vermiş olduğum userla ilgilili parolayı şifreleyip kaydetme işlemini yapacak

				if (identityResult.Succeeded)//succeded işin başarılı olup olmaması errors hatalı olma durumu
				{
					var memberRole = await _roleManager.FindByNameAsync("Member");
					if (memberRole == null)
					{
						await _roleManager.CreateAsync(new AppRole()
						{ //kullanıcıya admin yetkisi vererek kaydetmek istedim.
							Name = "Member",
							CretedTime = DateTime.Now,
						});
					}
					

					await _userManager.AddToRoleAsync(user, "Member");
					return RedirectToAction("Index", "Home");
				}

				foreach (var error in identityResult.Errors)
				{
					ModelState.AddModelError("", error.Description); //bu hataları görmek için create.cshtmle girip div etiketiyle en alta hata kısmı asp.val. eklemek gerek
				}
			}
			return View(model);


		}

		public IActionResult SignIn(string returnUrl) //diyelim direk home/adminpanel yazdık, bizi cookie entg. gereği signIne attı. O sırada returnUrl yazısı çıkmıştı.O bu. içinde home/Adminpanel var
		{


			return View(new UserSignInModel { ReturnUrl = returnUrl} );
		}
		[HttpPost]
		public async Task<IActionResult> SignIn(UserSignInModel model)
		{//signinResult (alttaki) sadece bir sonuç döndürür. İşlem doğruysa doğru yanlışsa yanlış. Islockedout= kullanıcının hesabı kilitli mi değil mi, IsNotAllowed=hesaba giriş yapıldı ama emailine link atılsın, ona tıkladıktan sonra doğrulansın.Doğrulanmışsa girsin ya da telefon numarası aynı şekilde.


			if (ModelState.IsValid)
			{

				var user = await _userManager.FindByNameAsync(model.UserName);
				var signInResult = await _signInManager.PasswordSignInAsync(model.UserName, model.Password,model.RememberMe /*false*/, true);//parametrelerinden ispPersistent mesela 5 dakika içinde sayfayı kapatsanız bile siteye direk sign olabiliyorsunuz tekrar girmeden. Lockoutonfailure ise ilgili kullanıcının hesabı kilitlensin mi, kilitlenmesin mi. Mesela kullanıcı belli haklarda şifreyi üst üste yanlış girerse belli süreliğine onu bloklayabiliyoruz.
				if (signInResult.Succeeded)
				{//bu iş başarılıysa
					if (!string.IsNullOrWhiteSpace(model.ReturnUrl)) //boş ya da null değilse
					{
						return Redirect(model.ReturnUrl); //redirect deyinse sadece url istiyor
					}

					//var user = await _userManager.FindByNameAsync(model.UserName); //elimde ilgili user var
					 var roles = await _userManager.GetRolesAsync(user); //ilgili Userın rol bilgisi var.

					if (roles.Contains("Admin"))
					{
						return RedirectToAction("AdminPanel");
					}
					else
					{
						return RedirectToAction("Panel");
					}

				}

				else if (signInResult.IsLockedOut)
				{ //12.59, 13.04 direk dakikaları çıkarırsam sıkıntı çıkarır gibi

					var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user); //  ne zamana kadar kilitliyim ?
					
					
					ModelState.AddModelError("", $"Hesabınız güvenlik amaçlı {(lockoutEnd.Value.UtcDateTime-DateTime.UtcNow).Minutes} dk askıya alınmıştır");
				}

				else
				{
					var message = string.Empty;


					if (user != null)
					{
						var failedCount = await _userManager.GetAccessFailedCountAsync(user);   //ilgili usrın hatalı girişlerinin sayısı
						message = $"{_userManager.Options.Lockout.MaxFailedAccessAttempts - failedCount} kez daha girerseniz hesabınız kalıcı olarak kilitlenecektir";
					}
					else
					{
						message = "Kullanıcı adı veya şifre hatalıdır.";
					}

					ModelState.AddModelError("", message);


				}

				//	else if (signInResult.IsLockedOut)
				//	{

				//	}
				//	else if (signInResult.IsNotAllowed)
				//	{//email veya phone number doğrulanmış mı

				//	}
				//}


			}
			return View(model);
		}

		[Authorize/*(Roles ="Admin")*/] //sadece giriş yapmış kullanıcılar erişsin, role eklersem rolü admin olan kulanıcılar erişsin. virgül ekleyip member da yazarsak, kişinin bu endpointe erişmesi için admin ve member olması gerekli deriz
		public IActionResult GetUserInfo() //buraya sadece giriş yapmış kullanıcılar erişsin.
		{

			var userName = User.Identity.Name; //özel bir classs var ve bu classla kullanıcının datalarına erişebiliyoruz demiştik, altta bunu yapalım.uSER BU
			var role = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value; //claimtype'ı role olan bilgiyi bana getir dediğimiz zaman ilgili user'ın rolüne de ulaşabiliriz
			User.IsInRole("Member");// bu rola sahip mi ? kontrolü llayouta yapabilirim. oraya yazdık.
			return View();
		}


		[Authorize(Roles ="Admin")]
		public IActionResult AdminPanel()
		{
			return View();
		}
		[Authorize(Roles ="Member")]
		public IActionResult Panel()
		{
			return View();
		}

		[Authorize(Roles = "Member")]
		public IActionResult MemberPage()
		{
			return View();
		}

		public async Task<IActionResult> SignOut()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index");
		}

		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
