using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("PhienBanTaiLieu")]
public partial class PhienBanTaiLieu
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaTaiLieu { get; set; }

    public int PhienBan { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? DinhDang { get; set; }

    public long KichThuoc { get; set; }

    [StringLength(64)]
    [Unicode(false)]
    public string HashFile { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string ObjectKeyR2 { get; set; } = null!;

    public bool? DaMaHoa { get; set; }

    public byte? TrangThaiUpload { get; set; }

    public Guid MaNguoiCapNhat { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    [ForeignKey("MaNguoiCapNhat")]
    [InverseProperty("PhienBanTaiLieus")]
    public virtual NguoiDung MaNguoiCapNhatNavigation { get; set; } = null!;

    [ForeignKey("MaTaiLieu")]
    [InverseProperty("PhienBanTaiLieus")]
    public virtual TaiLieu MaTaiLieuNavigation { get; set; } = null!;
}
