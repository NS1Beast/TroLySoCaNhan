using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("NhatKyHeThong")]
public partial class NhatKyHeThong
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid? MaNguoiDung { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string LoaiHanhDong { get; set; } = null!;

    [Column("DiaChiIP")]
    [StringLength(50)]
    [Unicode(false)]
    public string? DiaChiIp { get; set; }

    public DateTime? ThoiGian { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("NhatKyHeThongs")]
    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
