using Berk.Identity.Context;
using Berk.Identity.Entities;
using Berk.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Berk.Identity.Controllers
{
	[Authorize(Roles ="Admin")] //Indexe ulaşmak için giriş yapmak zorunda
	//rolü admib olmayan userları görmesi lazım, rolü member olan userların çekilmesi gibi. Context'ten
	public class UserController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<AppRole> _roleManager;
		private readonly BerkContext _context;

		public UserController(UserManager<AppUser> userManager, BerkContext context, RoleManager<AppRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			//Users ilk yazdığımızda IQuareyable dönüyor.Bu database işlemlerini yapabileceğimiz anlamına geliyor. Ben burdan rahatça sorgu yazacağım

			//  var query = _userManager.Users;
			//var users = _context.Users.Join(_context.UserRoles, user => user.Id, userRole => userRole.UserId, (user, userRole) => new
			//{ //Ccontrextin içinden userrolesleri birleştireceğim, userı Id alanıyla userroleun userrole Id'si ile birleştireceğim.  Bir de benim admin ıd 1. 1 OLMAYAN KAYITLARI GETİRMEK İSTEYECEĞİM

			//	//users = admin olmayan userlar.

			//	user,
			//	userRole
			//}).Join(_context.Roles, two => two.userRole.RoleId, role => role.Id, (two, role) => new { two.user, two.userRole, role })

			//.Where(x => x.role.Name != "Admin").Select(x => new AppUser
			//{ //rolü admin olmayanları getirmek istiyorsam başa bir join daha

			//	Id = x.user.Id,
			//	AccessFailedCount = x.user.AccessFailedCount,
			//	ConcurrencyStamp = x.user.ConcurrencyStamp,
			//	Email = x.user.Email,
			//	EmailConfirmed = x.user.EmailConfirmed,
			//	Gender = x.user.Gender,
			//	ImagePath = x.user.ImagePath,
			//	LockoutEnabled = x.user.LockoutEnabled,
			//	LockoutEnd = x.user.LockoutEnd,
			//	NormalizedEmail = x.user.NormalizedEmail,
			//	NormalizedUserName = x.user.NormalizedUserName,
			//	PasswordHash = x.user.PasswordHash,
			//	PhoneNumber = x.user.PhoneNumber,
			//	UserName = x.user.UserName,
			//	PhoneNumberConfirmed = x.user.PhoneNumberConfirmed,


			//}).ToList(); //1 e eşit olmayan kayıtları getir, getirirken de AppUsera bind ederek getir demiştik. Yukarda da güncelledik.

			////var users = await _userManager.GetUsersInRoleAsync("Member");

			//return View(users);

			List<AppUser> filterdUser = new List<AppUser>();
			var users = _userManager.Users.ToList();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				if (!roles.Contains("Admin"))
				{
					filterdUser.Add(user);
				}

				
			}
			return View(users);
		}

		public IActionResult Create()
		{
			return View(new UserAdminCreateModel());
		}

		[HttpPost]
		public async Task<IActionResult> Create(UserAdminCreateModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new AppUser() {

					Email = model.Email,
					Gender = model.Gender,
					UserName = model.UserName,

				};

				var result = await _userManager.CreateAsync(user, model.UserName+ "123"); //paswword olarakta modelin içindeki username'in sonuna 123 ekleyerek oluşsun
				

				if (result.Succeeded)
				{
					var memberrole = await _roleManager.FindByNameAsync("Member");
					if (memberrole == null)
					{
						await _roleManager.CreateAsync(new AppRole()
						{
							Name ="Member",
							CretedTime = DateTime.Now,

						});
					}
					await _userManager.AddToRoleAsync(user, "Member");
					return RedirectToAction("Index");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

			}
			return View(model);
		}

		public async Task<IActionResult> AssignRole(int id) //bu parameterenin içindeki user id
		{
			var user = _userManager.Users.SingleOrDefault(x => x.Id == id);
			var userRoles = await  _userManager.GetRolesAsync(user);
			var roles = _roleManager.Roles.ToList(); //bu tüm roller

			RoleAssignSendModel model = new RoleAssignSendModel();
			List<RoleAssignListModel> list = new List<RoleAssignListModel>();

			foreach (var role in roles)
			{
				list.Add(new RoleAssignListModel()
				{
					Name = role.Name,
					RoleId = role.Id,
					Exist = userRoles.Contains(role.Name) //userRollerimde varsa içeriyorsa gelen role'ün Nameini içeriyorsa.

				});

				
			}

			model.Roles = list;
			model.UserId = id;

			return View(model);

		}

		[HttpPost]
		public async Task<IActionResult> AssignRole(RoleAssignSendModel model)
		{ //userda rol atama işleminde böyle bir rol varsa hiçbir şey yapmayacağım, kaldırdığı şey için eğer ilgili userımda kaldırdığı rol varsa sileceğim.
		  //role ekleme = seçilen rolün ilgili userda olmaması gerek
		  //role çıkartma = seçilen rolün ilgili userda olması gerek

			var user = _userManager.Users.SingleOrDefault(x => x.Id == model.UserId);
			var userRoles =  await _userManager.GetRolesAsync(user);

			foreach (var role in model.Roles)
			{
				if (role.Exist) //role seçiliyse
				{
					if (!userRoles.Contains(role.Name)) //role existse ekleme yapıyorum. role eklerken seçilen rolün olmaması gerek
					{
						await _userManager.AddToRoleAsync(user, role.Name); //usera burdaki rolü ekle
					}
					else
					{
						if (userRoles.Contains(role.Name))
							await _userManager.RemoveFromRoleAsync(user, role.Name);
						{

						}
					}
				}

			}

			return RedirectToAction("Index");
		}

	}
}
