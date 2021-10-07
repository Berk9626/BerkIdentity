using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Berk.Identity.Models
{
	public class UserAdminCreateModel
	{
		[Required(ErrorMessage ="Kullanıcı adı gereklidir")]
		public string UserName { get; set; }
		[Required(ErrorMessage = "Email gereklidir")]
		public string Email { get; set; }
		[Required(ErrorMessage = "Cinsiyet gereklidir")]
		public string Gender { get; set; }

	}
}
