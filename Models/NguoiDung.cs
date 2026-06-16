using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("NguoiDung")]
[Index("MaNgauNhien", Name = "UQ__NguoiDun__44F670656665F16B", IsUnique = true)]
[Index("TenDangNhap", Name = "UQ__NguoiDun__55F68FC0D68A3FA4", IsUnique = true)]
[Index("Email", Name = "UQ__NguoiDun__A9D10534650CA036", IsUnique = true)]
public partial class NguoiDung
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TenDangNhap { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? MatKhauHash { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string MaNgauNhien { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(100)]
    public string TenHienThi { get; set; } = null!;

    [Column("KhoaCongKhaiPGP")]
    public string? KhoaCongKhaiPgp { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SoDuVi { get; set; }

    public long? DungLuongToiDa { get; set; }

    [Column("LuotAISuDung")]
    public int? LuotAisuDung { get; set; }

    public byte? VaiTro { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual CaiDatNguoiDung? CaiDatNguoiDung { get; set; }

    [InverseProperty("MaNguoiNhanNavigation")]
    public virtual ICollection<ChiaSeTaiLieuCaNhan> ChiaSeTaiLieuCaNhans { get; set; } = new List<ChiaSeTaiLieuCaNhan>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<DanhBa> DanhBaMaNguoiDungNavigations { get; set; } = new List<DanhBa>();

    [InverseProperty("MaNguoiLienHeNavigation")]
    public virtual ICollection<DanhBa> DanhBaMaNguoiLienHeNavigations { get; set; } = new List<DanhBa>();

    [InverseProperty("MaChuSoHuuNavigation")]
    public virtual ICollection<DanhMuc> DanhMucs { get; set; } = new List<DanhMuc>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<LichSuGiaoDich> LichSuGiaoDiches { get; set; } = new List<LichSuGiaoDich>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<NhatKyHeThong> NhatKyHeThongs { get; set; } = new List<NhatKyHeThong>();

    [InverseProperty("MaNguoiThucHienNavigation")]
    public virtual ICollection<NhatKyTaiLieu> NhatKyTaiLieus { get; set; } = new List<NhatKyTaiLieu>();

    [InverseProperty("MaNguoiTaoNavigation")]
    public virtual ICollection<NhomLuuTru> NhomLuuTrus { get; set; } = new List<NhomLuuTru>();

    [InverseProperty("MaNguoiCapNhatNavigation")]
    public virtual ICollection<PhienBanTaiLieu> PhienBanTaiLieus { get; set; } = new List<PhienBanTaiLieu>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<TacVuNen> TacVuNens { get; set; } = new List<TacVuNen>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<TaiKhoanLienKet> TaiKhoanLienKets { get; set; } = new List<TaiKhoanLienKet>();

    [InverseProperty("MaChuSoHuuNavigation")]
    public virtual ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();

    [InverseProperty("MaNguoiDungNavigation")]
    public virtual ICollection<ThanhVienNhom> ThanhVienNhoms { get; set; } = new List<ThanhVienNhom>();

    [InverseProperty("MaNguoiNhanNavigation")]
    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
