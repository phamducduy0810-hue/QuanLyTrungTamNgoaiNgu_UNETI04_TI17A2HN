using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    // Họ và tên: Diem Duc Hung
    // Mã sinh viên: 23103100099
    // Nội dung thực hiện: Module 3 - Quản lý học viên
    public class HocVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHocVien { get; set; }

        [Required]
        public int MaTaiKhoan { get; set; }
        [ForeignKey("MaTaiKhoan")]
        public virtual TaiKhoan? TaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên bắt buộc nhập.")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Ngày sinh bắt buộc nhập.")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [StringLength(10)]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại bắt buộc nhập.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15)]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email bắt buộc nhập.")]
        [EmailAddress(ErrorMessage = "Email đúng định dạng.")]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(255)]
        public string DiaChi { get; set; }

        public virtual ICollection<DangKyHoc> DangKyHocs { get; set; } = new List<DangKyHoc>();
    }
}
