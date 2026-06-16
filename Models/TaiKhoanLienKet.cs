using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("TaiKhoanLienKet")]
[Index("ThongTinDangNhap", Name = "UQ__TaiKhoan__FA032FAEDD07EEE2", IsUnique = true)]
public partial class TaiKhoanLienKet
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaNguoiDung { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string NenTang { get; set; } = null!;

    [StringLength(255)]
    public string ThongTinDangNhap { get; set; } = null!;

    public bool? TrangThai { get; set; }

    public DateTime? NgayLienKet { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("TaiKhoanLienKets")]
    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
