using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    // Họ và tên: Diem Duc Hung
    // Mã sinh viên: 23103100099
    // Nội dung thực hiện: Đăng ký học và Quản lý đăng ký
    public class DangKyHoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDangKy { get; set; }

        [Required]
        public int MaHocVien { get; set; }
        [ForeignKey("MaHocVien")]
        public virtual HocVien? HocVien { get; set; }

        [Required]
        public int MaLop { get; set; }
        [ForeignKey("MaLop")]
        public virtual LopHoc? LopHoc { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayDangKy { get; set; }

        [Required]
        public decimal HocPhi { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ xác nhận"; // Chờ xác nhận, Đã xác nhận, Đã hủy
    }
}
