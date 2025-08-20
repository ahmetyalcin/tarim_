using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TarimHibe.Models;

namespace TarimHibe.Models
{



    public class Arazi
    {

        [Key]
        public int Id { get; set; }
        public int UserID { get; set; }
        public int MulkiyetId { get; set; }  // AraziMulkiyet tablosuyla ilişkili
        public int AraziTurId { get; set; } // AraziTurleri tablosuyla ilişkili
        public int AraziSulamaId { get; set; }    // AraziSulama tablosuyla ilişkili
        public int AraziKullanimId { get; set; }
        public string Ilce { get; set; }
        public string Mahalle { get; set; }

        public string IlceAdi { get; set; }
        public string MahalleAdi { get; set; }

        public string Ada { get; set; }
        public string Parsel { get; set; }
        public string Mevkii { get; set; }
        public decimal HisseAlani { get; set; }
        public decimal ToplamAlani { get; set; }
        public DateTime? KayitTarihi { get; set; }

        // Navigation Properties

        [BindNever] // veri bağlama bunlara
        public AraziMulkiyet MulkiyetBilgisi { get; set; }
        [BindNever]
        public AraziTurleri AraziTuruBilgisi { get; set; }
        [BindNever]
        public AraziSulama SulamaBilgisi { get; set; }
        [BindNever]
        public List<CKS_Bilgileri> CKSBelgeleri { get; set; }

        public ICollection<CksBelgesi> CksBelgesiArsivi { get; set; } = new List<CksBelgesi>();

    }

    // Diğer modeller (DB'den otomatik generate edilebilir)
    public class AraziMulkiyet 
    {
        [Key]
        public int MulkiyetId { get; set; }
        public string MulkiyetAdi { get; set; }
    }
    public class AraziTurleri
    {
        [Key]
        public int AraziTurId { get; set; }
        public string AraziTurAdi { get; set; } 
    
    }
    public class AraziSulama 
    {
        [Key]
        public int AraziSulamaId { get; set; } 
        public string AraziSulamaAdi { get; set; }
    }

    public class AraziKullanim
    {
        [Key]
        public int AraziKullanimId { get; set; }
        public string AraziKullanimAdi { get; set; }
    }


    public class CKS_Bilgileri {


        [Key]
        public int CKSID { get; set; }
        public int? AraziId { get; set; }
        [ForeignKey(nameof(AraziId))]
        public Arazi? Arazi { get; set; } = null!;

        public int UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public Users User { get; set; } = null!;


        public string BelgeDosyasi { get; set; } = null!;
        public int Yil { get; set; }
        public int OnayDurumu { get; set; }
        public DateTime KayitTarihi { get; set; }

    }

    public class AdminHibeBasvuruViewModel
    {
        public int BasvuruID { get; set; }
        public string HibeTuru { get; set; }
        public bool UcretliMi { get; set; }
        public string BasvuranAdi { get; set; }
        public string AraziBilgi { get; set; }
        public DateTime BasvuruTarihi { get; set; }
        public string DekontYolu { get; set; }
        public string BasvuruDurumu { get; set; }
        public bool AdminOnay { get; set; }
    }

    public class CksOnayViewModel
    {
        public int CksID { get; set; }
        public int Yil { get; set; }
        public string Kullanici { get; set; } = "";
        public string AraziBilgi { get; set; } = "";
        public string BelgeYolu { get; set; } = "";
        public int OnayDurumu { get; set; }    // 0=Beklemede,1=Onaylandı,2=Reddedildi
    }

    public class CksBelgesi
    {
        [Key]
        public int Id { get; set; }

        // Kullanıcıya ait olacaksa:
        public int UserId { get; set; }
        public Users User { get; set; }


        public DateTime YuklemeTarihi { get; set; }
        public string DosyaYolu { get; set; }
        public bool OnaylandiMi { get; set; }
        public bool IsActive { get; set; }
    }

}
