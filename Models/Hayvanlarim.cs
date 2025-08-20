using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TarimHibe.Models
{
    public class Hayvanlarim
    {
        [Key]
        public int Id { get; set; }
        public int? UserID { get; set; }
        public int? HayGrupID { get; set; }
        public int? HayAltGrupID { get; set; }
        public int? HayTurID { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string Mahalle { get; set; }
        public int Adet { get; set; }
        public string Durum { get; set; }
        public DateTime? KayitTarihi { get; set; }

        [ForeignKey("HayAltGrupID")]
        public virtual HayvanAltGruplari HayvanAltGruplari { get; set; }

        [ForeignKey("UserID")]
        public virtual Users Users { get; set; }

        [ForeignKey("HayTurID")]
        public virtual HayvanTurleri HayvanTurleri { get; set; }

        [ForeignKey("HayGrupID")]
        public virtual HayvanTuruGruplari HayvanTuruGruplari { get; set; }
    }



    public class HayvanViewModel
    {
        [Key]
        public int Id { get; set; }
        public string GrupAdi { get; set; }
        public string AltGrupAdi { get; set; }
        public string TurAdi { get; set; }
        public int Ilce { get; set; }
        public int Mahalle { get; set; }
        public int Adet { get; set; }
        public string Durum { get; set; }
        public int? UserID { get; set; }
        public int? HayGrupID { get; set; }
        public int? HayAltGrupID { get; set; }
        public int? HayTurID { get; set; }
        public int Il { get; set; }
        public DateTime? KayitTarihi { get; set; }
        public virtual HayvanAltGruplari HayvanAltGruplari { get; set; }
        public virtual Users Users { get; set; }
        public virtual HayvanTurleri HayvanTurleri { get; set; }
        public virtual HayvanTuruGruplari HayvanTuruGruplari { get; set; }
    }



    public class HayvanTurleri
    {
        [Key]
        public int TurID { get; set; }
        public int? AltGrupID { get; set; }
        public string TurAdi { get; set; }

        [ForeignKey("AltGrupID")]
        public HayvanAltGruplari HayvanAltGruplari { get; set; }
        public ICollection<Hayvanlarim> Hayvanlarim { get; set; } = new HashSet<Hayvanlarim>();
    }

    public class HayvanAltGruplari
    {
        [Key]
        public int AltGrupID { get; set; }
        public int? GrupID { get; set; }
        public string AltGrupAdi { get; set; }

        [ForeignKey("GrupID")]
        public HayvanTuruGruplari HayvanTuruGruplari { get; set; }
        public ICollection<HayvanTurleri> HayvanTurleri { get; set; } = new HashSet<HayvanTurleri>();
        public ICollection<Hayvanlarim> Hayvanlarim { get; set; } = new HashSet<Hayvanlarim>();
    }

    public class HayvanTuruGruplari
    {
        [Key]
        public int GrupID { get; set; }
        public string GrupAdi { get; set; }

    }
}
