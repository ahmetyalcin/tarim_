using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace TarimHibe.Models
{
    public class Users
    {
        [Key] 
        public int UserID { get; set; }
        public string TcKimlikNo { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public System.DateTime DogumTarihi { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public string Parola { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string Mahalle { get; set; }
        public Nullable<System.DateTime> KayitTarihi { get; set; }
        public ICollection<HibeBasvuru> HibeBasvurular { get; set; }
          = new List<HibeBasvuru>();
        public int RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;

        // UserMenu nav prop
        public virtual ICollection<UserMenu> UserMenus { get; set; }
            = new List<UserMenu>();
    }

    public class UserViewModel
    {
        public int UserID { get; set; }
        public string TcKimlik { get; set; } = "";
        public string AdSoyad { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleName { get; set; } = "";
    }

    public class KpsResponse
    {
        public long tcKimlikNo { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string babaAd { get; set; }
        public string anneAd { get; set; }
        public string cinsiyet { get; set; }
        public string adres { get; set; }
        public bool adresDurum { get; set; }
        public string il { get; set; }
        public string ilce { get; set; }
        public string mahalleKoy { get; set; }
        public string csbm { get; set; }
        public string hata { get; set; }
    }

    // KPS API Request modeli
    public class KpsRequest
    {
        public string tcKimlikNo { get; set; }
        public int dogumGun { get; set; }
        public int dogumAy { get; set; }
        public int dogumYil { get; set; }
        public string ipAdresi { get; set; }
        public string kullaniciAdi { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "TC Kimlik zorunlu.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "11 haneli TC Kimlik girin.")]
        [Display(Name = "TC Kimlik")]
        public string TcKimlikNo { get; set; } = "";

        [Required(ErrorMessage = "Doğum tarihi zorunlu.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]

        public DateTime DogumTarihi { get; set; }

        public static ValidationResult ValidateBirthDate(DateTime date, ValidationContext ctx)
        {
            if (date >= DateTime.Today)
                return new ValidationResult("Doğum tarihi bugün veya sonrasını ifade edemez.");
            return ValidationResult.Success;
        }

        [Required(ErrorMessage = "Ad zorunlu.")]
        public string Ad { get; set; } = "";

        [Required(ErrorMessage = "Soyad zorunlu.")]
        public string Soyad { get; set; } = "";

        [Required(ErrorMessage = "Telefon zorunlu.")]
        [RegularExpression(@"^\d{3}\s\d{3}\s\d{2}\s\d{2}$",
            ErrorMessage = "Telefon formatı: 505 123 45 67")]
        public string Telefon { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunlu.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Parola zorunlu.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Parola en az 6 karakter olmalı.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,20}$", ErrorMessage = "Parola en az 1 harf ve 1 rakam içermeli, sadece harf ve rakam kullanılmalı.")]
        [DataType(DataType.Password)]
        




        public string Parola { get; set; } = "";

        [Required(ErrorMessage = "Parola (Tekrar) zorunlu.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Parola), ErrorMessage = "Parolalar eşleşmiyor.")]
        [Display(Name = "Parola (Tekrar)")]
        public string ParolaTekrar { get; set; } = "";

        [Range(typeof(bool), "true", "true", ErrorMessage = "KVKK onayını işaretleyin.")]
        [Display(Name = "KVKK Onayı")]
        public bool KvkKOnay { get; set; }

        // reCaptcha token
        public string RecaptchaToken { get; set; } = "";
    }

    public class ProfileViewModel
    {
        // User fields
        public int UserID { get; set; }

        [Required]
        [Display(Name = "Ad")] public string Ad { get; set; } = "";
        [Required]
        [Display(Name = "Soyad")] public string Soyad { get; set; } = "";
        [Required]
        [Phone]
        [Display(Name = "Telefon")] public string Telefon { get; set; } = "";
        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")] public string Email { get; set; } = "";
        [Required]
        [Display(Name = "İl")] public string Il { get; set; } = "";
        [Required]
        [Display(Name = "İlçe")] public string Ilce { get; set; } = "";
        [Required]
        [Display(Name = "Mahalle")] public string Mahalle { get; set; } = "";

        // Password change
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Parola")] public string CurrentPassword { get; set; } = "";
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Parola")] public string NewPassword { get; set; } = "";
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Parolalar eşleşmiyor")]
        [Display(Name = "Yeni Parola (Tekrar)")] public string ConfirmPassword { get; set; } = "";
    }

}
