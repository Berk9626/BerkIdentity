using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Berk.Identity.Entities
{
	public class AppUser : IdentityUser<int>
	{
		public string ImagePath { get; set; }
		//3NF'ye aykırı. Ben bir alanı günceleldiğim zaman, tüm tablodaki userları güncellememem için...Şimdilik buraya takılmayalım.
		public string Gender { get; set; }
	}
}
