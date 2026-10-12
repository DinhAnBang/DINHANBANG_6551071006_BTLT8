using Microsoft.EntityFrameworkCore;

namespace CAU1.Models;

// Model va DbContext nay duoc viet tuong thich voi Database First.
// Neu ban da scaffold xong, co the giu nguyen hai file do thay vi ghi de.
public partial class TiemTapHoaContext : DbContext
{
    public TiemTapHoaContext() { }
    public TiemTapHoaContext(DbContextOptions<TiemTapHoaContext> options) : base(options) { }

    public virtual DbSet<HangHoa> HangHoas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HangHoa>(entity =>
        {
            entity.HasKey(e => e.MaHang).HasName("PK_HangHoa");
            entity.ToTable("HangHoa");
            entity.Property(e => e.MaHang).ValueGeneratedOnAdd();
            entity.Property(e => e.TenHang).HasMaxLength(100);
            entity.Property(e => e.LoaiHang).HasMaxLength(50);
            entity.Property(e => e.DonVi).HasMaxLength(20);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.NgayNhap).HasColumnType("datetime");
        });
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
