using Newtonsoft.Json;

namespace TarimHibe.Models.ApiModels
{
    public class IlceModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("adi")]
        public string IlceAdi { get; set; }

        // This property is used for the view but not in deserialization
        public string IlceKodu => Id.ToString();
    }
}