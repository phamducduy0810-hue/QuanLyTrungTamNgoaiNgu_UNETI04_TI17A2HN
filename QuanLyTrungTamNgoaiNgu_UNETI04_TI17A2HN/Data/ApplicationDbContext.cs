using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<KhoaHoc> KhoaHocs { get; set; }
        public DbSet<LopHoc> LopHocs { get; set; }
        public DbSet<HocVien> HocViens { get; set; }
        public DbSet<DangKyHoc> DangKyHocs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            
            modelBuilder.Entity<HocVien>()
                .HasOne(h => h.TaiKhoan)
                .WithOne(t => t.HocVien)
                .HasForeignKey<HocVien>(h => h.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<LopHoc>()
                .HasOne(l => l.KhoaHoc)
                .WithMany(k => k.LopHocs)
                .HasForeignKey(l => l.MaKhoaHoc)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<DangKyHoc>()
                .HasOne(d => d.HocVien)
                .WithMany(h => h.DangKyHocs)
                .HasForeignKey(d => d.MaHocVien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DangKyHoc>()
                .HasOne(d => d.LopHoc)
                .WithMany(l => l.DangKyHocs)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
