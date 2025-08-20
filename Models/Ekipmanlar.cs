using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TarimHibe.Models;

namespace TarimHibe.Models
{

    public class Ekipmanlar1Test
    {

        [Key]
        public int EkipmanID { get; set; }
        public int? UserID { get; set; }
        public string EkipmanAdi { get; set; }
        public int? TurID { get; set; }
        public string Plaka { get; set; }
        public string Marka { get; set; }
        public string Modeli { get; set; }
        public string Ilce { get; set; }
        public string Mahalle { get; set; }
        public int Adet { get; set; }
        public string Durum { get; set; }
        public DateTime? KayitTarihi { get; set; }

        // 🔗 İlişkiler (navigation properties)
        [ForeignKey("TurID")]
        public virtual EkipmanTurleri EkipmanTurleri { get; set; }

        [ForeignKey("UserID")]
        public virtual Users Users { get; set; }



    }


    public class Ekipmanlar
    {
        [Key]
        public int EkipmanID { get; set; }
        public int? UserID { get; set; }
        public string EkipmanAdi { get; set; }
        public int? TurID { get; set; }
        public string Plaka { get; set; }
        public string Marka { get; set; }
        public string Modeli { get; set; }
        public string Ilce { get; set; }
        public string Mahalle { get; set; }
        public int Adet { get; set; }
        public string Durum { get; set; }
        public DateTime? KayitTarihi { get; set; }

        // 🔗 İlişkiler (navigation properties)
        [ForeignKey("TurID")]
        public virtual EkipmanTurleri EkipmanTurleri { get; set; }

        [ForeignKey("UserID")]
        public virtual Users Users { get; set; }
    }

    public class EkipmanTurleri
    {
        [Key]
        public int TurID { get; set; }

        public string TurAdi { get; set; }

        // 🔗 İlişkili ekipmanlar listesi
        public virtual ICollection<Ekipmanlar> Ekipmanlar { get; set; } = new List<Ekipmanlar>();
    }


}
