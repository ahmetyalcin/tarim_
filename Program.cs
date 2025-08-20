using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using TarimHibe.Filters;
using TarimHibe.Models;
using TarimHibe.Services;

var builder = WebApplication.CreateBuilder(args);

// DI servisleri
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<IzmirBBService>();

builder.Services.AddScoped<INotificationService, NotificationService>();


builder.Services.AddHttpClient<ICbsService, CbsService>(client =>
{
    client.BaseAddress = new Uri(
        "https://cbs.izmir.bel.tr/cbswebservisleri/CbsKurumSozelServisiWebApi/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Session & HttpContext
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// EF Core
builder.Services.AddDbContext<HibeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HibeConnection")));

// MVC + Global filtreler
builder.Services.AddControllersWithViews(opts =>
{
    // 1) Oturum kontrolü
    opts.Filters.Add<AuthenticationFilter>();
    // 2) Global login kontrolünden sonra, roller için parametresiz ctor
    opts.Filters.Add(new AuthorizeRoleAttribute());
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
