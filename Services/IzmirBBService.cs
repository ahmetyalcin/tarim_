using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TarimHibe.Models.ApiModels;
using System.Diagnostics;


namespace TarimHibe.Services
{
    public class IzmirBBService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://ibbgateway.izmir.bel.tr/apicore";
        private const string LicenseKey = "YniJOVgAOXaGpCwldZoa/Q==";
        private string _token;
        private DateTime _tokenExpireDate;

        public IzmirBBService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task EnsureValidTokenAsync()
        {
            try
            {
                Debug.WriteLine("Checking token validity...");
                if (string.IsNullOrEmpty(_token) || DateTime.Now >= _tokenExpireDate)
                {
                    Debug.WriteLine("Token is null or expired. Getting new token...");
                    await GetNewTokenAsync();
                }
                else
                {
                    Debug.WriteLine("Token is valid. Continuing...");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Token validation error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new Exception("Token doðrulama hatasý: " + ex.Message, ex);
            }
        }

        private async Task GetNewTokenAsync()
        {
            try
            {
                Debug.WriteLine("Preparing token request...");

                _httpClient.DefaultRequestHeaders.Authorization = null;

                var request = new HttpRequestMessage(HttpMethod.Get, "https://ibbgateway.izmir.bel.tr/apicore/izbbkimlik-v2/GetToken");
                request.Headers.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("LICENSE-KEY", LicenseKey);

                Debug.WriteLine("Sending token request...");
                Debug.WriteLine($"Request URI: {request.RequestUri}");
                Debug.WriteLine($"License Key: {LicenseKey}");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Token response status: {response.StatusCode}");
                Debug.WriteLine($"Token response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Token alýnamadý. Status: {response.StatusCode}, Content: {content}");
                }

                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<TokenResponse>>(content);

                    if (apiResponse?.Result == null || string.IsNullOrEmpty(apiResponse.Result.AccessToken))
                    {
                        throw new Exception("Token yanýtý boþ veya geçersiz");
                    }

                    _token = apiResponse.Result.AccessToken;
                    _tokenExpireDate = apiResponse.Result.DateExpired;

                    Debug.WriteLine($"Token successfully obtained. Expires: {_tokenExpireDate}");

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token);
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine($"JSON parsing error: {jsonEx.Message}");
                    Debug.WriteLine($"Response content that failed to parse: {content}");
                    throw new Exception("Token yanýtý iþlenemedi", jsonEx);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetNewToken error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new Exception("Token alýnamadý: " + ex.Message, ex);
            }
        }

        public async Task<List<IlceModel>> GetIlcelerAsync()
        {
            try
            {
                Debug.WriteLine("Getting ilceler...");
                await EnsureValidTokenAsync();

                Debug.WriteLine("Making request to /disservisler/InnosaWaOrt/IlceKodlariGetir");
                Debug.WriteLine($"Authorization: Bearer {_token}");

                var response = await _httpClient.GetAsync("https://ibbgateway.izmir.bel.tr/apicore/disservisler/InnosaWaOrt/IlceKodlariGetir");
                var content = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Ilceler response status: {response.StatusCode}");
                Debug.WriteLine($"Raw Ilceler response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ýlçeler alýnamadý. Status: {response.StatusCode}, Content: {content}");
                }

                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<IlceModel>>>(content);
                    Debug.WriteLine($"Deserialized API Response: {JsonConvert.SerializeObject(apiResponse, Formatting.Indented)}");

                    if (apiResponse?.Result != null)
                    {
                        Debug.WriteLine($"Successfully retrieved {apiResponse.Result.Count} ilceler");
                        foreach (var ilce in apiResponse.Result)
                        {
                            Debug.WriteLine($"Ilce: Id={ilce.Id}, Ad={ilce.IlceAdi}");
                        }
                        return apiResponse.Result;
                    }

                    throw new Exception("Ýlçe listesi boþ veya geçersiz format");
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine($"JSON parsing error: {jsonEx.Message}");
                    Debug.WriteLine($"Response content that failed to parse: {content}");
                    throw new Exception("Ýlçe verisi iþlenemedi: " + jsonEx.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetIlceler error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new Exception("Ýlçeler alýnamadý: " + ex.Message, ex);
            }
        }

        public async Task<List<MahalleModel>> GetMahallelerAsync(string ilceId)
        {
            try
            {
                Debug.WriteLine($"Getting mahalleler for ilce {ilceId}...");
                await EnsureValidTokenAsync();

              

                var response = await _httpClient.GetAsync($"https://ibbgateway.izmir.bel.tr/apicore/disservisler/InnosaWaOrt/MahalleKodlariGetir/{ilceId}");
                var content = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Mahalleler response status: {response.StatusCode}");
                Debug.WriteLine($"Mahalleler response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Mahalleler alýnamadý. Status: {response.StatusCode}, Content: {content}");
                }

                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MahalleModel>>>(content);
                    if (apiResponse?.Result != null && apiResponse.Result.Count > 0)
                    {
                        Debug.WriteLine($"Successfully retrieved {apiResponse.Result.Count} mahalleler");
                        return apiResponse.Result;
                    }

                    throw new Exception("Mahalle listesi boþ veya geçersiz format");
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine($"JSON parsing error: {jsonEx.Message}");
                    Debug.WriteLine($"Response content that failed to parse: {content}");
                    throw new Exception("Mahalle verisi iþlenemedi: " + jsonEx.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMahalleler error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new Exception("Mahalleler alýnamadý: " + ex.Message, ex);
            }
        }
    }
}