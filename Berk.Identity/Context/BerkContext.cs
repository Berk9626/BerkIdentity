using Berk.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Berk.Identity.Context
{
	public class BerkContext : IdentityDbContext<AppUser, AppRole, int>//senin T User identityUser değil artık AppUser.Senin Trole ıdentity role değil AppRole, diğer tablolarının primarykey'i string değil int artık          //Bir confg. üretmek istiyorsak IdentityUSer'ı kullanmalıyız.
	{
		//userlogin: 3.parti yazılım kullanıldığında(googl,microsoft) bu uygulmayla giriş yapma özelliğini tutar
		//userclaims: Users tablosu haricinde ek bilgileri tuttuğumuz tablo
		//role tablosu zaten biliyoruz.role claims ise role bilgileri haricinde tutulması gereken bilgilerin yer aldığı kısım
		//NetUsers tablosu içindeki normalizedusername ve normalizedemail, diyelim ki kullancı tablom var ve bunda milyonlarca kayıt var.UserName'i getir diye bir sorgu attım desek bu sorgunun sonucu uzuzn sürebileceği için bu iki yer indexlenmiş.Böylece sonuç geliyor. Girilen her harfi büyüğe çeviriyor.
		//EmailConfirmed alanı ilgili userın emaili doğrulanmış mı doğrulanmamış mı datası
		//passwordhash bir kullanıcının şifresini doğrudan databasede tutmayız. Tablo ele geçerse doğrudan ilgili kullanıcının şifresini karşıya göstermek doğru olmaz.
		//concurrencystamp, ben sistem kurguladım, bu sistemde username önemli bnim için, bunun değişimine izin vermiyorum diyelim. Eğer kullanıcı bu UserNAME'İ DEĞİŞTİRMEK isterse bir ticket açılıyor ve bu sistemde yönetici olarak çalışan yönetici kişiye bildiriliyor.Admin paneline düşer.
		//ph.confirmed, tel no doğrulaması
		//twofactorenabled, iki aşamalı doğrulamada telefon noya sms gönerme vs.
		//locoutenabled, kilitlenme feature özelliği açık mı kapalı mı ?, bu alanın true olması, ilgili kişinin hesabının kitli olduğu anlamına gelmiyor. Bu kullanıcının başarısız bir şekilde mesela 5 kere yanlış girerse kullanıcı adını(accesFailedCount) hesabı kilitlensin.İşte hesabı kilitlenme özelliği açık mı yoksa kapalı mı olsun ? Eğer true girersek belirtmiş olduğumuz Accesfailedcounta ulaştığında identity, lockoutende kadar hesabı kitleyecektir.



		public BerkContext(DbContextOptions<BerkContext> options): base(options)
		{

		}
	}
}
