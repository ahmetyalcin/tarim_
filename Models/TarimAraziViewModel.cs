using System.ComponentModel.DataAnnotations;

namespace TarimHibe.Models
{
    // Models/TarimAraziViewModel.cs
    public class TarimAraziViewModel1
    {
        // Adım 1
        [Required] public string Il { get; set; }
        [Required] public string Ilce { get; set; }
        [Required] public string Mahalle { get; set; }
        [Required] public string Ada { get; set; }
        [Required] public string Parsel { get; set; }

        // Adım 2
        [Required] public string Mulkiyet { get; set; }
        [Required] public string AraziTuru { get; set; }
        [Required] public string Sulama { get; set; }
        [Required] public string KullanımSekli { get; set; }
        public string Mevkii { get; set; }
        [Required] public decimal ToplamAlan { get; set; }
        [Required] public decimal HisseAlan { get; set; }

        // Adım 3
        [Required] public string TCNo { get; set; }
        [Required][DataType(DataType.Date)] public DateTime DogumTarihi { get; set; }
        [Required] public int UretimYili { get; set; }
        [Required] public IFormFile CksBelgesi { get; set; }
    }
}
