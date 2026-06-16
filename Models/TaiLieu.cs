using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("TaiLieu")]
[Index("MaChuSoHuu", "MaNhomLuuTru", "DaXoa", "NgayTao", Name = "IDX_TaiLieu_CaNhan_Pagination", IsDescending = new[] { false, false, false, true })]
[Index("MaNhomLuuTru", "DaXoa", "NgayTao", Name = "IDX_TaiLieu_Nhom_Pagination", IsDescending = new[] { false, false, true })]
public partial class TaiLieu
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(255)]
    public string TenTaiLieu { get; set; } = null!;

    public Guid MaChuSoHuu { get; set; }

    public Guid? MaNhomLuuTru { get; set; }

    public bool? DaXoa { get; set; }

    public DateTime? NgayTao { get; set; }

    [InverseProperty("MaTaiLieuNavigation")]
    public virtual ICollection<ChiaSeTaiLieuCaNhan> ChiaSeTaiLieuCaNhans { get; set; } = new List<ChiaSeTaiLieuCaNhan>();

    [ForeignKey("MaChuSoHuu")]
    [InverseProperty("TaiLieus")]
    public virtual NguoiDung MaChuSoHuuNavigation { get; set; } = null!;

    [ForeignKey("MaNhomLuuTru")]
    [InverseProperty("TaiLieus")]
    public virtual NhomLuuTru? MaNhomLuuTruNavigation { get; set; }

    [InverseProperty("MaTaiLieuNavigation")]
    public virtual ICollection<NhatKyTaiLieu> NhatKyTaiLieus { get; set; } = new List<NhatKyTaiLieu>();

    [InverseProperty("MaTaiLieuNavigation")]
    public virtual ICollection<PhienBanTaiLieu> PhienBanTaiLieus { get; set; } = new List<PhienBanTaiLieu>();

    [InverseProperty("MaTaiLieuGocNavigation")]
    public virtual ICollection<TacVuNenChiTiet> TacVuNenChiTietMaTaiLieuGocNavigations { get; set; } = new List<TacVuNenChiTiet>();

    [InverseProperty("MaTaiLieuKetQuaNavigation")]
    public virtual ICollection<TacVuNenChiTiet> TacVuNenChiTietMaTaiLieuKetQuaNavigations { get; set; } = new List<TacVuNenChiTiet>();

    [ForeignKey("MaTaiLieu")]
    [InverseProperty("MaTaiLieus")]
    public virtual ICollection<DanhMuc> MaDanhMucs { get; set; } = new List<DanhMuc>();
}
