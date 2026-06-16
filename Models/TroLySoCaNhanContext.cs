using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

public partial class TroLySoCaNhanContext : DbContext
{
    public TroLySoCaNhanContext()
    {
    }

    public TroLySoCaNhanContext(DbContextOptions<TroLySoCaNhanContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CaiDatNguoiDung> CaiDatNguoiDungs { get; set; }

    public virtual DbSet<ChiaSeTaiLieuCaNhan> ChiaSeTaiLieuCaNhans { get; set; }

    public virtual DbSet<DanhBa> DanhBas { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<GoiDichVu> GoiDichVus { get; set; }

    public virtual DbSet<LichSuGiaoDich> LichSuGiaoDiches { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }

    public virtual DbSet<NhatKyTaiLieu> NhatKyTaiLieus { get; set; }

    public virtual DbSet<NhomLuuTru> NhomLuuTrus { get; set; }

    public virtual DbSet<PhienBanTaiLieu> PhienBanTaiLieus { get; set; }

    public virtual DbSet<TacVuNen> TacVuNens { get; set; }

    public virtual DbSet<TacVuNenChiTiet> TacVuNenChiTiets { get; set; }

    public virtual DbSet<TaiKhoanLienKet> TaiKhoanLienKets { get; set; }

    public virtual DbSet<TaiLieu> TaiLieus { get; set; }

    public virtual DbSet<ThanhVienNhom> ThanhVienNhoms { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI;Database=TroLySoCaNhan;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaiDatNguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__CaiDatNg__C539D762B60893E5");

            entity.Property(e => e.MaNguoiDung).ValueGeneratedNever();
            entity.Property(e => e.ChuDeGiaoDien).HasDefaultValue("System");
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.NgonNgu).HasDefaultValue("vi-VN");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.ThongBaoApp).HasDefaultValue(true);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.CaiDatNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaiDat_NguoiDung");
        });

        modelBuilder.Entity<ChiaSeTaiLieuCaNhan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiaSeTa__3214EC27354AFF51");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayChiaSe).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Quyen).HasDefaultValue((byte)1);

            entity.HasOne(d => d.MaNguoiNhanNavigation).WithMany(p => p.ChiaSeTaiLieuCaNhans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiaSe_NguoiNhan");

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.ChiaSeTaiLieuCaNhans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiaSe_TaiLieu");
        });

        modelBuilder.Entity<DanhBa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhBa__3214EC277B51A9DC");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayKetBan).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)1);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhBaMaNguoiDungNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhBa_Chu");

            entity.HasOne(d => d.MaNguoiLienHeNavigation).WithMany(p => p.DanhBaMaNguoiLienHeNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhBa_Khach");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhMuc__3214EC27E6AD5D8E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");

            entity.HasOne(d => d.MaChuSoHuuNavigation).WithMany(p => p.DanhMucs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhMuc_User");
        });

        modelBuilder.Entity<GoiDichVu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GoiDichV__3214EC272D8E44B0");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<LichSuGiaoDich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LichSuGi__3214EC2772DDF3D4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayGiaoDich).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)2);

            entity.HasOne(d => d.MaGoiDichVuNavigation).WithMany(p => p.LichSuGiaoDiches).HasConstraintName("FK_LSGD_GoiDichVu");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.LichSuGiaoDiches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LSGD_User");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NguoiDun__3214EC2781DCFD88");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DungLuongToiDa).HasDefaultValueSql("((5368709120.))");
            entity.Property(e => e.LuotAisuDung).HasDefaultValue(100);
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.SoDuVi).HasDefaultValue(0m);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.VaiTro).HasDefaultValue((byte)1);
        });

        modelBuilder.Entity<NhatKyHeThong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NhatKyHe__3214EC27A121ECC1");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ThoiGian).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.NhatKyHeThongs).HasConstraintName("FK_NKHT_User");
        });

        modelBuilder.Entity<NhatKyTaiLieu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NhatKyTa__3214EC27373F7C2A");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayThucHien).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.MaNguoiThucHienNavigation).WithMany(p => p.NhatKyTaiLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NKTL_NguoiThucHien");

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.NhatKyTaiLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NKTL_TaiLieu");
        });

        modelBuilder.Entity<NhomLuuTru>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NhomLuuT__3214EC276E403ADE");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.MaNguoiTaoNavigation).WithMany(p => p.NhomLuuTrus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NhomLuuTru_NguoiTao");
        });

        modelBuilder.Entity<PhienBanTaiLieu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PhienBan__3214EC27AB8AC9D2");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DaMaHoa).HasDefaultValue(false);
            entity.Property(e => e.NgayCapNhat).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PhienBan).HasDefaultValue(1);
            entity.Property(e => e.TrangThaiUpload).HasDefaultValue((byte)0);

            entity.HasOne(d => d.MaNguoiCapNhatNavigation).WithMany(p => p.PhienBanTaiLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhienBan_NguoiSua");

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.PhienBanTaiLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhienBan_TaiLieu");
        });

        modelBuilder.Entity<TacVuNen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TacVuNen__3214EC271FBDCC57");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayBatDau).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.SoFileHoanThanh).HasDefaultValue(0);
            entity.Property(e => e.TiLeHoanThanh).HasDefaultValue((byte)0);
            entity.Property(e => e.TongSoFile).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)2);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.TacVuNens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TacVuNen_User");
        });

        modelBuilder.Entity<TacVuNenChiTiet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TacVuNen__3214EC27DE1BB094");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayThucHien).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.TienDoTungFile).HasDefaultValue((byte)0);
            entity.Property(e => e.TrangThai).HasDefaultValue((byte)2);

            entity.HasOne(d => d.MaTacVuNenNavigation).WithMany(p => p.TacVuNenChiTiets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTiet_TacVu");

            entity.HasOne(d => d.MaTaiLieuGocNavigation).WithMany(p => p.TacVuNenChiTietMaTaiLieuGocNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTiet_TaiLieuGoc");

            entity.HasOne(d => d.MaTaiLieuKetQuaNavigation).WithMany(p => p.TacVuNenChiTietMaTaiLieuKetQuaNavigations).HasConstraintName("FK_ChiTiet_TaiLieuKQ");
        });

        modelBuilder.Entity<TaiKhoanLienKet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaiKhoan__3214EC27DCB6198C");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.NgayLienKet).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.TaiKhoanLienKets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaiKhoanLienKet_NguoiDung");
        });

        modelBuilder.Entity<TaiLieu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaiLieu__3214EC27469C673E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.MaChuSoHuuNavigation).WithMany(p => p.TaiLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaiLieu_ChuSoHuu");

            entity.HasOne(d => d.MaNhomLuuTruNavigation).WithMany(p => p.TaiLieus).HasConstraintName("FK_TaiLieu_Nhom");

            entity.HasMany(d => d.MaDanhMucs).WithMany(p => p.MaTaiLieus)
                .UsingEntity<Dictionary<string, object>>(
                    "PhanLoaiTaiLieu",
                    r => r.HasOne<DanhMuc>().WithMany()
                        .HasForeignKey("MaDanhMuc")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PLTL_DanhMuc"),
                    l => l.HasOne<TaiLieu>().WithMany()
                        .HasForeignKey("MaTaiLieu")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PLTL_TaiLieu"),
                    j =>
                    {
                        j.HasKey("MaTaiLieu", "MaDanhMuc").HasName("PK__PhanLoai__962FF6DF7D1D4851");
                        j.ToTable("PhanLoaiTaiLieu");
                    });
        });

        modelBuilder.Entity<ThanhVienNhom>(entity =>
        {
            entity.HasKey(e => new { e.MaNhom, e.MaNguoiDung }).HasName("PK__ThanhVie__1F1C0CBB5F0A224E");

            entity.Property(e => e.NgayThamGia).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.QuyenHan).HasDefaultValue(1);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.ThanhVienNhoms)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhVienNhom_User");

            entity.HasOne(d => d.MaNhomNavigation).WithMany(p => p.ThanhVienNhoms)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhVienNhom_Nhom");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThongBao__3214EC27C53BE761");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DaDoc).HasDefaultValue(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.MaNguoiNhanNavigation).WithMany(p => p.ThongBaos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThongBao_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
