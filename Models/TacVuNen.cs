using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("TacVuNen")]
public partial class TacVuNen
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaNguoiDung { get; set; }

    public byte LoaiTacVu { get; set; }

    public string? NoiDungYeuCau { get; set; }

    public int? TongSoFile { get; set; }

    public int? SoFileHoanThanh { get; set; }

    public byte? TiLeHoanThanh { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("TacVuNens")]
    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    [InverseProperty("MaTacVuNenNavigation")]
    public virtual ICollection<TacVuNenChiTiet> TacVuNenChiTiets { get; set; } = new List<TacVuNenChiTiet>();
}
