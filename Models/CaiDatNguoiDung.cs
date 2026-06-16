using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("CaiDatNguoiDung")]
public partial class CaiDatNguoiDung
{
    [Key]
    public Guid MaNguoiDung { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? ChuDeGiaoDien { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? NgonNgu { get; set; }

    public bool? ThongBaoApp { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public DateTime? NgayCapNhat { get; set; }

    [ForeignKey("MaNguoiDung")]
    [InverseProperty("CaiDatNguoiDung")]
    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
