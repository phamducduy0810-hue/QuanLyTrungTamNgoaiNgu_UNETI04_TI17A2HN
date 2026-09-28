using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    public class DangKyHoc
    {
        [Key]
        public int MaDangKy { get; set; }
        [Required]
        public string MaHocVien { get; set; }
        [Required]
        public string MaLop { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayDangKy { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HocPhi { get; set; }
        [Required]
        public string TrangThai { get; set; } = "Chờ xác nhận";
        [ForeignKey("MaHocVien")]
        public HocVien? HocVien { get; set; }
        [ForeignKey("MaLop")]
        public LopHoc? LopHoc { get; set; }
    }
}
