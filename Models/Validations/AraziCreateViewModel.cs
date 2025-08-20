using System.ComponentModel.DataAnnotations;

namespace TarimHibe.Models.Validations
{
    public class AraziCreateViewModel
    {
        [Required(ErrorMessage = "Mülkiyet seçimi zorunludur")]
        [Display(Name = "Mülkiyet")]
        public int MulkiyetId { get; set; }

        [Required(ErrorMessage = "Arazi türü seçimi zorunludur")]
        [Display(Name = "Arazi Türü")]
        public int AraziTuruId { get; set; }

        [Required(ErrorMessage = "Sulama seçimi zorunludur")]
        [Display(Name = "Sulama")]
        public int SulamaId { get; set; }

        [Required(ErrorMessage = "Kullanım şekli zorunludur")]
        [Display(Name = "Kullanım Şekli")]
        public string KullanimSekli { get; set; } = null!;

        [Required(ErrorMessage = "İl seçimi zorunludur")]
        [Display(Name = "İl")]
        public string Il { get; set; } = null!;

        [Required(ErrorMessage = "İlçe seçimi zorunludur")]
        [Display(Name = "İlçe")]
        public string Ilce { get; set; } = null!;

        [Required(ErrorMessage = "Mahalle seçimi zorunludur")]
        [Display(Name = "Mahalle")]
        public string Mahalle { get; set; } = null!;

        [Required(ErrorMessage = "Ada numarası zorunludur")]
        [Display(Name = "Ada")]
        public string Ada { get; set; } = null!;

        [Required(ErrorMessage = "Parsel numarası zorunludur")]
        [Display(Name = "Parsel")]
        public string Parsel { get; set; } = null!;

        [Display(Name = "Mevkii")]
        public string? Mevkii { get; set; }

        [Required(ErrorMessage = "Hisse alanı zorunludur")]
        [Display(Name = "Hisse Alanı (da)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hisse alanı 0'dan büyük olmalıdır")]
        public decimal HisseAlani { get; set; }

        [Required(ErrorMessage = "Toplam alan zorunludur")]
        [Display(Name = "Toplam Alan (da)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Toplam alan 0'dan büyük olmalıdır")]
        public decimal ToplamAlani { get; set; }

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [Display(Name = "TC Kimlik No")]
        public string TcKimlikNo { get; set; } = null!;

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime DogumTarihi { get; set; }

        [Required(ErrorMessage = "ÇKS belgesi zorunludur")]
        [Display(Name = "ÇKS Belgesi")]
        public IFormFile CKSBelgesi { get; set; } = null!;

        [Required(ErrorMessage = "ÇKS yılı zorunludur")]
        [Display(Name = "ÇKS Yılı")]
        [Range(2023, 2025, ErrorMessage = "Geçerli bir ÇKS yılı seçiniz")]
        public int CKSYili { get; set; }
    }
}



