using Newtonsoft.Json;

namespace TarimHibe.Models.ApiModels
{
    public class MahalleModel
    {
        [JsonProperty("id")]
        public string MahalleKodu { get; set; }

        [JsonProperty("adi")]
        public string MahalleAdi { get; set; }
    }
}