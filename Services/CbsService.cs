using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TarimHibe.Models.ApiModels;
using System.Diagnostics;

namespace TarimHibe.Services
{
    public class CbsService : ICbsService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl =
             "https://cbs.izmir.bel.tr/cbswebservisleri/CbsKurumSozelServisiWebApi/";
        private string _token;
        private DateTime _tokenExpireDate;

        public CbsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept
                       .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task EnsureValidTokenAsync()
        {
            if (string.IsNullOrEmpty(_token) || DateTime.Now >= _tokenExpireDate)
                await GetTokenAsync();
        }

        private async Task GetTokenAsync()
        {
            // TODO: kullanıcı adı/şifreyi konfigürasyondan oku
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = "tarimsalhizmetler",
                ["password"] = "EHjXE3Zu7pzZ"
            };


            Debug.WriteLine($"Token URL: {_httpClient.BaseAddress}token");
            var response = await _httpClient.PostAsync("token",
                                    new FormUrlEncodedContent(form));
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"CBS Token status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"CBS token alınamadı: {response.StatusCode}");

            var obj = JObject.Parse(content);
            _token = obj.Value<string>("access_token");
            var expiresIn = obj.Value<int>("expires_in");
            _tokenExpireDate = DateTime.Now.AddSeconds(expiresIn - 60);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

        }

        public async Task<List<IlceModel>> GetIlcelerAsync()
        {
            await EnsureValidTokenAsync();

            using var req = new HttpRequestMessage(HttpMethod.Post, "api/MulkiyetServis/ilceGetir");
            using var resp = await _httpClient.SendAsync(req);
            var txt = await resp.Content.ReadAsStringAsync();
            Debug.WriteLine($"CBS İlçe status: {resp.StatusCode}");

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"CBS ilçe alınamadı: {resp.StatusCode}");

            var j = JObject.Parse(txt);
            if (j.Value<bool>("success") == false)
                throw new Exception(j["message"]?.ToString() ?? "CBS ilçe sorgu hatası");

            // Burada "message" bir liste
            return j["message"]!
                   .ToObject<List<IlceModel>>()
                   ?? new List<IlceModel>();
        }

        public async Task<List<MahalleModel>> GetMahallelerAsync(string ilceId)
        {
            await EnsureValidTokenAsync();

            using var req = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/MulkiyetServis/mahalleGetir?ilceid={ilceId}"
            );
            using var resp = await _httpClient.SendAsync(req);
            var txt = await resp.Content.ReadAsStringAsync();
            Debug.WriteLine($"CBS Mahalle status: {resp.StatusCode}");

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"CBS mahalle alınamadı: {resp.StatusCode}");

            var j = JObject.Parse(txt);
            if (j.Value<bool>("success") == false)
                throw new Exception(j["message"]?.ToString() ?? "CBS mahalle sorgu hatası");

            return j["message"]!
                   .ToObject<List<MahalleModel>>()
                   ?? new List<MahalleModel>();
        }

        // Services/CbsService.cs

        public async Task<JObject> GetParselAsync(string mahalleId, string ada, string parsel)
        {
            await EnsureValidTokenAsync();

            // POST isteği, sr=4326 ekledik:
            var resp = await _httpClient.PostAsync(
                $"api/MulkiyetServis/parselGetir?mahalleid={mahalleId}&ada={ada}&parsel={parsel}&sr=4326",
                null
            );
            var txt = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"CBS parsel sorgu hatası: {resp.StatusCode}");

            var root = JObject.Parse(txt);
            if (!root.Value<bool>("success"))
                throw new Exception(root["message"]?.ToString() ?? "CBS parsel sorgu başarısız");

            // message bir dizi, ilk öğeyi alıyoruz
            var msg = root["message"];
            if (msg is JArray arr && arr.Count > 0)
                return (JObject)arr[0]!;

            // tek obje geldiyse
            return (JObject)msg!;
        }

    }
}
