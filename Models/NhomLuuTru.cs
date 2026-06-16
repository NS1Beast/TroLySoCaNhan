using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("NhomLuuTru")]
public partial class NhomLuuTru
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(255)]
    public string TenNhom { get; set; } = null!;

    public Guid MaNguoiTao { get; set; }

    public DateTime? NgayTao { get; set; }

    [ForeignKey("MaNguoiTao")]
    [InverseProperty("NhomLuuTrus")]
    public virtual NguoiDung MaNguoiTaoNavigation { get; set; } = null!;

    [InverseProperty("MaNhomLuuTruNavigation")]
    public virtual ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();

    [InverseProperty("MaNhomNavigation")]
    public virtual ICollection<ThanhVienNhom> ThanhVienNhoms { get; set; } = new List<ThanhVienNhom>();
}
