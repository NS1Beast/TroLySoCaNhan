using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("ThongBao")]
[Index("MaNguoiNhan", "DaDoc", "NgayTao", Name = "IDX_ThongBao_Pagination", IsDescending = new[] { false, false, true })]
public partial class ThongBao
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaNguoiNhan { get; set; }

    [StringLength(100)]
    public string TieuDe { get; set; } = null!;

    [StringLength(500)]
    public string NoiDung { get; set; } = null!;

    public byte LoaiThongBao { get; set; }

    public bool? DaDoc { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? LinkHanhDong { get; set; }

    public DateTime? NgayTao { get; set; }

    [ForeignKey("MaNguoiNhan")]
    [InverseProperty("ThongBaos")]
    public virtual NguoiDung MaNguoiNhanNavigation { get; set; } = null!;
}
