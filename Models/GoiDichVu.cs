using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TroLySoCaNhan.Models;

[Table("GoiDichVu")]
public partial class GoiDichVu
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string TenGoi { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiaTien { get; set; }

    public long DungLuongTangThem { get; set; }

    [Column("LuotAITangThem")]
    public int LuotAitangThem { get; set; }

    public int ThoiHanNgay { get; set; }

    [InverseProperty("MaGoiDichVuNavigation")]
    public virtual ICollection<LichSuGiaoDich> LichSuGiaoDiches { get; set; } = new List<LichSuGiaoDich>();
}
