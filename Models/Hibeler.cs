using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TarimHibe.Models
{
    public class Hibe
    {
        [Key]
        public int HibeID { get; set; }

        public int? UserID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string HibeTuru { get; set; } = null!;

        [Required]
        public int Miktar { get; set; }

        [Required]
        public bool UcretliMi { get; set; }

        public DateTime? KayitTarihi { get; set; }

        public string? HibeAciklama { get; set; }

        [Required]
        public DateTime BaslangicTarihi { get; set; }

        [Required]
        public DateTime BitisTarihi { get; set; }

        public bool Yayinda { get; set; }
    }

    public class HibeBasvuru
    {
        [Key]
        public int BasvuruID { get; set; }

        public int HibeID { get; set; }
        [ForeignKey(nameof(HibeID))]
        public Hibe Hibe { get; set; } = null!;

        public int UserID { get; set; }
       
        /*[ForeignKey(nameof(UserID))]
        public ApplicationUser User { get; set; } = null!;
        */

        [ForeignKey(nameof(UserID))]
        public Users User { get; set; } = null!;


        public int AraziID { get; set; }
        [ForeignKey(nameof(AraziID))]
        public Arazi Arazi { get; set; } = null!;

        public DateTime BasvuruTarihi { get; set; }

        /// <summary>
        /// Ücretli hibe ise dekont dosyası. Null ise henüz yüklenmemiş.
        /// </summary>
    
        public string? Dekont { get; set; }

        /// <summary>
        /// 0 = dekont bekleniyor, 1 = ödendi/yüklendi
        /// </summary>
        public bool OdemeDurumu { get; set; }

        /// <summary>
        /// “Dekont Bekleniyor”, “İnceleniyor”, “Onaylandı”, “Reddedildi” vb.
        /// </summary>
        [StringLength(100)]
        public string BasvuruDurumu { get; set; } = "Dekont Bekleniyor";

        /// <summary>
        /// Admin onayı durumu: 0 = bekliyor, 1 = onaylandı
        /// </summary>
        public bool AdminOnayDurumu { get; set; }

        public DateTime KayitTarihi { get; set; }
    }

    public class HibeCardViewModel
    {
        public int HibeID { get; set; }
        public string HibeTuru { get; set; }
        public int Miktar { get; set; }
        public bool UcretliMi { get; set; }
        public bool Basvurdu { get; set; }
        public bool Yayinda { get; set; }

        public string? HibeAciklama { get; set; }
        public bool HasApprovedCks { get; set; }
        public bool AlreadyApplied { get; set; }
        public bool HasRegisteredArazi { get; set; }

    }

    public class HibeApplyViewModel
    {
        public int HibeID { get; set; }
        public string HibeTuru { get; set; }
        public int Miktar { get; set; }
        public bool UcretliMi { get; set; }
        public bool Yayinda { get; set; }
        public string? HibeAciklama { get; set; }
        public List<SelectListItem> AraziList { get; set; }
    }

    public class HibeApplyForm
    {
        public int HibeID { get; set; }
        public int AraziID { get; set; }
        public IFormFile? DekontFile { get; set; }
    }



    
        public class BasvuruViewModel
        {
            public int BasvuruID { get; set; }
            public string HibeTuru { get; set; } = "";
            public string AraziBilgi { get; set; } = "";
            public DateTime BasvuruTarihi { get; set; }
           public string BasvuruDurumu { get; set; }

        public bool AdminOnayDurumu { get; set; }

                    // Durum’u buna göre üret
                    public string Durum => AdminOnayDurumu
                        ? "Onaylandı"
                        : "Beklemede";
    }



}
