// Họ và tên: Phạm Văn Công 
// Mã sinh viên: 23103100115
// Nội dung thực hiện: Module 1 - Entity TaiKhoan

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã tài khoản")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Mật khẩu ít nhất 4 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò không được để trống")]
        [StringLength(20)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "HocVien"; // Admin, HocVien

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true: Hoạt động, false: Khóa

        // Navigation Property 1-1 với HocVien
        public virtual HocVien? HocVien { get; set; }
    }
}
