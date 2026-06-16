using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("LichSuGiaoDich")]
public partial class LichSuGiaoDich
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaNguoiDung { get; set; }

    public byte LoaiGiaoDich { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SoTien { get; set; }

    public Guid? MaGoiDichVu { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? PhuongThuc { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? NgayGiaoDich { get; set; }

    [ForeignKey("MaGoiDichVu")]
    [InverseProperty("LichSuGiaoDiches")]
    public virtual GoiDichVu? MaGoiDichVuNavigation { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("LichSuGiaoDiches")]
    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
