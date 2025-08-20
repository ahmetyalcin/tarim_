using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using TarimHibe.Models;    // DbContext ve entity’lerin namespace’i
using TarimHibe.Helpers;   // HashPassword için
using TarimHibe.Filters;   // AuthorizeRoleAttribute burada
using System;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace TarimHibe.Controllers
{
    public class AuthController : Controller
    {
        private readonly HibeDbContext _context;
        private readonly IHttpClientFactory _http;

        // API bilgileri
        private const string KpsApiUrl = "https://intrarest1.izbb.net/TarimKps/Kps/KisiBilgileriniSorgula";
        private const string KpsUsername = "svc_iyi_tarim";
        private const string KpsPassword = "Mavi2025ab";
        private const string RecaptchaSecret = "6LclekUrAAAAAAEjd91ZJ1pklT-G4sR2K3Sgocf4";

        public AuthController(HibeDbContext context, IHttpClientFactory http)
        {
            _context = context;
            _http = http;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Login(string TcKimlikNo, string Parola)
        {
            try
            {
                string hashedPassword = SecurityHelper.HashPassword(Parola);
                var user = _context.Users
           .Include(u => u.Role)
           .FirstOrDefault(u =>
               u.TcKimlikNo == TcKimlikNo &&
               u.Parola == hashedPassword
           );

                if (user != null)
                {
                    HttpContext.Session.SetString("Role", user.Role.RoleName);
                    HttpContext.Session.SetString("UserId", user.UserID.ToString());
                    HttpContext.Session.SetString("AdSoyad", user.Ad + " " + user.Soyad);
                    HttpContext.Session.SetString("Telefon", user.Telefon ?? "");
                    HttpContext.Session.SetString("TcKimlikNo", user.TcKimlikNo);

                    if (user.Role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Index", "Dashboard");
                    else
                        return RedirectToAction("UserIndex", "Dashboard");
                }

                TempData["ErrorMessage"] = "Geçersiz TC Kimlik No veya şifre!";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction("Login");
            }
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Register()
        {
            var vm = new RegisterViewModel
            {
                DogumTarihi = DateTime.Today
            };
            return View(vm);
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel m)
        {
            if (!ModelState.IsValid)
                return View(m);

            try
            {
                // 1) KPS API ile kişi bilgilerini sorgula ve İzmir ikamet kontrolü yap
                var kpsResult = await VerifyPersonWithKps(m.TcKimlikNo, m.DogumTarihi);

                if (kpsResult == null)
                {
                    TempData["ErrorMessage"] = "Kişi bilgileri doğrulanamadı. Lütfen TC Kimlik No ve doğum tarihinizi kontrol edin. Sadece İzmir'de ikamet eden vatandaşlar kayıt olabilir.";
                    return View(m);
                }

                // İzmir ikamet kontrolü
                if (!kpsResult.adresDurum)
                {
                    TempData["ErrorMessage"] = "Üzgünüz, sadece İzmir'de ikamet eden vatandaşlar kayıt olabilir.";
                    return View(m);
                }

                if (!string.IsNullOrEmpty(kpsResult.hata))
                {
                    TempData["ErrorMessage"] = $"Doğrulama hatası: {kpsResult.hata}";
                    return View(m);
                }

         

                // KPS'den gelen ad-soyad ile form bilgilerini karşılaştır (Türkçe karakterler dahil)
                if (!IsNameMatch(kpsResult.ad, m.Ad) || !IsNameMatch(kpsResult.soyad, m.Soyad))
                {
                    TempData["ErrorMessage"] = "Girilen ad ve soyad bilgileri TC Kimlik kayıtlarınızla uyuşmuyor.";
                    return View(m);
                }

            
                // 2) reCAPTCHA v3 doğrulama (isteğe bağlı)
                var client = _http.CreateClient();
                var resp = await client.PostAsync(
                    "https://www.google.com/recaptcha/api/siteverify",
                    new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string,string>("secret", RecaptchaSecret),
                    new KeyValuePair<string,string>("response", m.RecaptchaToken)
                    }));
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var success = doc.RootElement.GetProperty("success").GetBoolean();
                var score = doc.RootElement.GetProperty("score").GetDecimal();
                if (!success || score < 0.5m)
                {
                    ModelState.AddModelError("", "reCAPTCHA doğrulaması başarısız.");
                    return View(m);
                }
             

                // 3) Email/TcKimlik unique kontrol
                if (_context.Users.Any(u => u.Email == m.Email))
                    ModelState.AddModelError(nameof(m.Email), "Bu e-posta zaten kayıtlı.");
                if (_context.Users.Any(u => u.TcKimlikNo == m.TcKimlikNo))
                    ModelState.AddModelError(nameof(m.TcKimlikNo), "Bu TC Kimlik zaten kayıtlı.");
                if (!ModelState.IsValid)
                    return View(m);

                // 4) Kullanıcı oluştur - KPS'den gelen adres bilgileriyle
                var user = new Users
                {
                    TcKimlikNo = m.TcKimlikNo,
                    Ad = kpsResult.ad, // KPS'den gelen doğru ad
                    Soyad = kpsResult.soyad, // KPS'den gelen doğru soyad
                    DogumTarihi = m.DogumTarihi,
                    Telefon = m.Telefon,
                    Email = m.Email,
                    Il = kpsResult.il ?? "-", // KPS'den gelen il bilgisi
                    Ilce = kpsResult.ilce ?? "-", // KPS'den gelen ilçe bilgisi
                    Mahalle = kpsResult.mahalleKoy ?? "-", // KPS'den gelen mahalle bilgisi
                    Parola = SecurityHelper.HashPassword(m.Parola),
                    KayitTarihi = DateTime.UtcNow,
                    RoleId = _context.Roles.Single(r => r.RoleName == "User").RoleId
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 5) Default menü ataması
                int[] defaultMenuIds = { 14, 18, 20, 10, 7, 9 };
                foreach (var mid in defaultMenuIds)
                {
                    _context.UserMenus.Add(new UserMenu
                    {
                        UserId = user.UserID,
                        MenuItemId = mid
                    });
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("RegisterSuccess");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                // Log the exception
                // _logger.LogError(ex, "Register işleminde hata");
                return View(m);
            }
        }

        private async Task<KpsResponse> VerifyPersonWithKps(string tcKimlikNo, DateTime dogumTarihi)
        {
            try
            {
                var client = _http.CreateClient();

                // Basic Auth header
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{KpsUsername}:{KpsPassword}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

                // Request body
                var request = new KpsRequest
                {
                    tcKimlikNo = tcKimlikNo,
                    dogumGun = dogumTarihi.Day,
                    dogumAy = dogumTarihi.Month,
                    dogumYil = dogumTarihi.Year,
                    ipAdresi = GetClientIpAddress(),
                    kullaniciAdi = "nihat_tavsan" // Sabit kullanıcı adı
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(KpsApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var kpsResponse = JsonSerializer.Deserialize<KpsResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return kpsResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "KPS API sorgulama hatası");
                return null;
            }
        }

        private string GetClientIpAddress()
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Eğer X-Forwarded-For header'ı varsa onu kullan (load balancer/proxy durumunda)
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }
            // X-Real-IP header'ı da kontrol et
            else if (HttpContext.Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            return ipAddress ?? "127.0.0.1";
        }

        // Türkçe karakterleri dikkate alarak ad-soyad karşılaştırması
        private bool IsNameMatch(string kpsName, string formName)
        {
            if (string.IsNullOrWhiteSpace(kpsName) || string.IsNullOrWhiteSpace(formName))
                return false;

            // Her iki ismi de normalize et (büyük harf + Türkçe karakter dönüşümü)
            var normalizedKpsName = NormalizeTurkishText(kpsName.Trim());
            var normalizedFormName = NormalizeTurkishText(formName.Trim());

            return string.Equals(normalizedKpsName, normalizedFormName, StringComparison.OrdinalIgnoreCase);
        }

        // Türkçe karakterleri ASCII karşılıklarına çevir ve büyük harfe dönüştür
        private string NormalizeTurkishText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var normalized = text.ToUpperInvariant();
            normalized = normalized.Replace('Ç', 'C')
                                 .Replace('Ğ', 'G')
                                 .Replace('İ', 'I')
                                 .Replace('Ö', 'O')
                                 .Replace('Ş', 'S')
                                 .Replace('Ü', 'U')
                                 .Replace('ç', 'C')
                                 .Replace('ğ', 'G')
                                 .Replace('ı', 'I')
                                 .Replace('i', 'I')
                                 .Replace('ö', 'O')
                                 .Replace('ş', 'S')
                                 .Replace('ü', 'U');

            return normalized;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
