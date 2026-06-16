using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("DanhMuc")]
public partial class DanhMuc
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    public Guid MaChuSoHuu { get; set; }

    [StringLength(100)]
    public string TenDanhMuc { get; set; } = null!;

    [ForeignKey("MaChuSoHuu")]
    [InverseProperty("DanhMucs")]
    public virtual NguoiDung MaChuSoHuuNavigation { get; set; } = null!;

    [ForeignKey("MaDanhMuc")]
    [InverseProperty("MaDanhMucs")]
    public virtual ICollection<TaiLieu> MaTaiLieus { get; set; } = new List<TaiLieu>();
}
