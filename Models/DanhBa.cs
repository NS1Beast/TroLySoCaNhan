using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("DanhBa")]
[Index("MaNguoiDung", "MaNguoiLienHe", Name = "UQ_DanhBa_LienHe", IsUnique = true)]
public partial class DanhBa
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaNguoiDung { get; set; }

    public Guid MaNguoiLienHe { get; set; }

    [StringLength(100)]
    public string? TenGoiNho { get; set; }

    public byte? TrangThai { get; set; }

    public DateTime? NgayKetBan { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("DanhBaMaNguoiDungNavigations")]
    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    [ForeignKey("MaNguoiLienHe")]
    [InverseProperty("DanhBaMaNguoiLienHeNavigations")]
    public virtual NguoiDung MaNguoiLienHeNavigation { get; set; } = null!;
}
