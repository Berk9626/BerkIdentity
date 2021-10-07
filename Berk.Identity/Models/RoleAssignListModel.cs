using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Berk.Identity.Models
{
	public class RoleAssignListModel //bütün rol bilgisi
	{
		public int RoleId { get; set; } //ilgili rolün Id'si
		public string Name { get; set; }
		public bool Exist { get; set; } //bu rolün userda olup olmadığını tutacağım property
	}

	public class RoleAssignSendModel
	{
		public List<RoleAssignListModel> Roles { get; set; }
		public int UserId { get; set; } //hangi userlara ben bunları enkleyip çıkartacağım
	}
}
