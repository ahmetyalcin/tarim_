using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace TarimHibe.Models
{
    public class AdminDashboardViewModel
    {
        public int ToplamKullanici { get; set; }
        public int ToplamHibeBasvuru { get; set; }
        public int ToplamArazi { get; set; }
        public int ToplamHayvan { get; set; }
        public int ToplamEkipman { get; set; }

        // Yeni eklenenler:
        public string EnCokBasvuruHibeAdi { get; set; }
        public int EnCokBasvuruAdedi { get; set; }

        // Son 10 çiftçi
        public List<CiftciItem> Son10Ciftci { get; set; } = new();

        public class CiftciItem
        {
            public string AdSoyad { get; set; }
            public string Email { get; set; }
            public DateTime KayitTarihi { get; set; }
        }



    }

    public class DashboardViewModel
    {
        public int HibeBasvuruSayisi { get; set; }
        public int AraziSayisi { get; set; }
        public int HayvanSayisi { get; set; }
        public int EkipmanSayisi { get; set; }

        // Bu yıla ait onaylı ÇKS belgesi var mı?
        public bool HasCurrentCks { get; set; }

        // Eklenen: açık hibeler
        public List<HibeMiniViewModel> YeniHibeler { get; set; } = new();

        public class HibeMiniViewModel
        {
            public int HibeID { get; set; }
            public int Miktar { get; set; }
            public string HibeTuru { get; set; } = string.Empty;
            public DateTime BaslangicTarihi { get; set; }
            public DateTime BitisTarihi { get; set; }
            public bool UcretliMi { get; set; }
        }


        // Hero Section için yeni özellikler
        public string HeroTitle { get; set; } = "Tarım Hibeleri Yönetim Sistemi";
        public string HeroSubtitle { get; set; } = "Tarım, bu şehrin bereketi ve geleceğidir. Çiftçilerimizin her zaman yanında olmak için modern tarım teknolojilerini ve sürdürülebilir üretimi destekliyoruz. Hedefimiz; daha verimli topraklar, daha güçlü bir üretim ve kendi kendine yetebilen bir şehir.”";
        public string HeroImageUrl { get; set; } = "https://images.unsplash.com/photo-1574323347407-f5e1ad6d020b?w=400&h=300&fit=crop";
        public string WelcomeBadgeText { get; set; } = "İyi Tarım'a Hoş Geldiniz!";

        // Slide/Haberler için
        public List<SlideItemViewModel> SlideItems { get; set; } = new();

        // Stats kartları için trend bilgileri
        public string HibeBasvuruTrend { get; set; } = "stable"; // "up", "down", "stable"
        public string AraziTrend { get; set; } = "stable";
        public string HayvanTrend { get; set; } = "stable";
        public string EkipmanTrend { get; set; } = "stable";


        public class SlideItemViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public string BackgroundColor { get; set; } = "#2c5530";
            public string LinkUrl { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
            public bool IsActive { get; set; } = true;
        }

    }

    public class TrendPoint
    {
        public DateTime Tarih { get; set; }
        public int UcretliCount { get; set; }
        public int UcretsizCount { get; set; }
    }

    public class RecentHibe
    {
        public string HibeTuru { get; set; } = string.Empty;
        public DateTime Baslangic { get; set; }
        public bool UcretliMi { get; set; }
    }

    // Dosyanın sonuna bu sınıfları ekleyin
    [Table("HeroSettings")]
    public class HeroSetting
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Subtitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string WelcomeBadgeText { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }

    [Table("SlideItems")]
    public class SlideItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string BackgroundColor { get; set; } = "#2c5530";

        [MaxLength(500)]
        public string LinkUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
