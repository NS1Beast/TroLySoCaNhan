using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("TacVuNen_ChiTiet")]
public partial class TacVuNenChiTiet
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaTacVuNen { get; set; }

    public Guid MaTaiLieuGoc { get; set; }

    public Guid? MaTaiLieuKetQua { get; set; }

    public byte? TrangThai { get; set; }

    public byte? TienDoTungFile { get; set; }

    public string? ChiTietLoi { get; set; }

    public DateTime? NgayThucHien { get; set; }

    [ForeignKey("MaTacVuNen")]
    [InverseProperty("TacVuNenChiTiets")]
    public virtual TacVuNen MaTacVuNenNavigation { get; set; } = null!;

    [ForeignKey("MaTaiLieuGoc")]
    [InverseProperty("TacVuNenChiTietMaTaiLieuGocNavigations")]
    public virtual TaiLieu MaTaiLieuGocNavigation { get; set; } = null!;

    [ForeignKey("MaTaiLieuKetQua")]
    [InverseProperty("TacVuNenChiTietMaTaiLieuKetQuaNavigations")]
    public virtual TaiLieu? MaTaiLieuKetQuaNavigation { get; set; }
}
