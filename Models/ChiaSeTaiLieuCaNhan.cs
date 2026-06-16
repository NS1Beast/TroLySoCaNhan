using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("ChiaSeTaiLieu_CaNhan")]
public partial class ChiaSeTaiLieuCaNhan
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaTaiLieu { get; set; }

    public Guid MaNguoiNhan { get; set; }

    public byte? Quyen { get; set; }

    public string? FileKeyDaMaHoa { get; set; }

    public DateTime? NgayChiaSe { get; set; }

    [ForeignKey("MaNguoiNhan")]
    [InverseProperty("ChiaSeTaiLieuCaNhans")]
    public virtual NguoiDung MaNguoiNhanNavigation { get; set; } = null!;

    [ForeignKey("MaTaiLieu")]
    [InverseProperty("ChiaSeTaiLieuCaNhans")]
    public virtual TaiLieu MaTaiLieuNavigation { get; set; } = null!;
}
