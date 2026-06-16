using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("NhatKyTaiLieu")]
public partial class NhatKyTaiLieu
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaTaiLieu { get; set; }

    public Guid MaNguoiThucHien { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string HanhDong { get; set; } = null!;

    public DateTime? NgayThucHien { get; set; }

    [ForeignKey("MaNguoiThucHien")]
    [InverseProperty("NhatKyTaiLieus")]
    public virtual NguoiDung MaNguoiThucHienNavigation { get; set; } = null!;

    [ForeignKey("MaTaiLieu")]
    [InverseProperty("NhatKyTaiLieus")]
    public virtual TaiLieu MaTaiLieuNavigation { get; set; } = null!;
}
