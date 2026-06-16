using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[PrimaryKey("MaNhom", "MaNguoiDung")]
[Table("ThanhVienNhom")]
public partial class ThanhVienNhom
{
    [Key]
    public Guid MaNhom { get; set; }

    [Key]
    public Guid MaNguoiDung { get; set; }

    public int QuyenHan { get; set; }

    public DateTime? NgayThamGia { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("ThanhVienNhoms")]
    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;

    [ForeignKey("MaNhom")]
    [InverseProperty("ThanhVienNhoms")]
    public virtual NhomLuuTru MaNhomNavigation { get; set; } = null!;
}
