using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TarimHibe.Models;


namespace TarimHibe.Models
{
    public class HibeDbContext : DbContext
    {
        /*
        public HibeDbContext(DbContextOptions<HibeDbContext> options) : base(options)
        {
        }
        */
        public HibeDbContext(DbContextOptions<HibeDbContext> opts)
        : base(opts) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<MenuRole> MenuRoles { get; set; }



        // Yeni DbSet'ler - Hero ve Slide yönetimi için
        public virtual DbSet<HeroSetting> HeroSettings { get; set; }
        public virtual DbSet<SlideItem> SlideItems { get; set; }


        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Hibe> Hibeler { get; set; }

        public DbSet<HibeBasvuru> HibeBasvurular { get; set; }


        public DbSet<Users> Users { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        

        public DbSet<UserMenu> UserMenus { get; set; }

        public DbSet<Ekipmanlar> Ekipmanlar { get; set; }



        public DbSet<EkipmanTurleri> EkipmanTurleri { get; set; }

        public DbSet<Hayvanlarim> Hayvanlarim { get; set; }

        public DbSet<HayvanViewModel> HayvanViewModel { get; set; }
        public DbSet<HayvanTurleri> HayvanTurleri { get; set; }
        public DbSet<HayvanTuruGruplari> HayvanTuruGruplari { get; set; }
        public DbSet<HayvanAltGruplari> HayvanAltGruplari { get; set; }



        public DbSet<AraziMulkiyet> AraziMulkiyet { get; set; }
        public DbSet<AraziTurleri> AraziTurleri { get; set; }
        public DbSet<AraziSulama> AraziSulama { get; set; }
        public DbSet<AraziKullanim> AraziKullanim { get; set; }
        

        public DbSet<Arazi> Araziler { get; set; }

        public DbSet<CKS_Bilgileri> CKS_Bilgileri { get; set; }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users tablosu
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.UserID);
            });

            // HibeBasvuru → Users ilişkisi
            modelBuilder.Entity<HibeBasvuru>(entity =>
            {
                entity.HasOne(b => b.User)
                      .WithMany(u => u.HibeBasvurular)
                      .HasForeignKey(b => b.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Menü öğeleri hiyerarşisi
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasMany(m => m.Children)
                      .WithOne()
                      .HasForeignKey(m => m.TopMenuId)
                      .IsRequired(false);
            });

            // Lookup tabloların PK tanımları
            modelBuilder.Entity<AraziMulkiyet>().HasKey(a => a.MulkiyetId);
            modelBuilder.Entity<AraziTurleri>().HasKey(a => a.AraziTurId);

            // Arazi ilişkileri
            modelBuilder.Entity<Arazi>(entity =>
            {
                entity.HasOne(a => a.MulkiyetBilgisi)
                      .WithMany()
                      .HasForeignKey(a => a.MulkiyetId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.AraziTuruBilgisi)
                      .WithMany()
                      .HasForeignKey(a => a.AraziTurId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CKS_Bilgileri ilişkileri
            modelBuilder.Entity<CKS_Bilgileri>(entity =>
            {
                entity.HasKey(c => c.CKSID);

                entity.HasOne(c => c.Arazi)
                      .WithMany(a => a.CKSBelgeleri)
                      .HasForeignKey(c => c.AraziId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany()  // Eğer Users sınıfında CKSBelgeleri listesi eklerseniz: .WithMany(u => u.CKSBelgeleri)
                      .HasForeignKey(c => c.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.ToTable("MenuItems");
                entity.HasKey(x => x.Id);

                entity.HasMany(m => m.Children)
                      .WithOne(m => m.Parent)
                      .HasForeignKey(m => m.TopMenuId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // Role ↔ Users
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // MenuRole ↔ Role
            modelBuilder.Entity<MenuRole>()
                .HasOne(mr => mr.Role)
                .WithMany(r => r.MenuRoles)
                .HasForeignKey(mr => mr.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HeroSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("HeroSettings");
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Subtitle).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.WelcomeBadgeText).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<SlideItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("SlideItems");
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.BackgroundColor).HasMaxLength(50).HasDefaultValue("#2c5530");
                entity.Property(e => e.LinkUrl).HasMaxLength(500);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(1);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });


        }

    }
}
