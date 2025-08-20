using System;
using Newtonsoft.Json;

namespace TarimHibe.Models.ApiModels
{
    public class TokenResponse
    {
        [JsonProperty("dateExpired")]
        public DateTime DateExpired { get; set; }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
}