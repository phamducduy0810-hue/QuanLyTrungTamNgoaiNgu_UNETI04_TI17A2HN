// Họ và tên: Diêm Đức Hùng
// Mã sinh viên: 23103100099
// Nội dung thực hiện: Module 3 - Entity GiangVien (Quản lý thông tin giảng viên)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    [Table("GiangVien")]
    public class GiangVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã giảng viên")]
        public int MaGiangVien { get; set; }

        [Display(Name = "Mã tài khoản")]
        public int? MaTaiKhoan { get; set; }

        [ForeignKey(nameof(MaTaiKhoan))]
        [Display(Name = "Tài khoản")]
        public virtual TaiKhoan? TaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ và tên giảng viên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; } = "Nam";

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Trình độ / Học vị")]
        public string? TrinhDo { get; set; } // Cử nhân, Thạc sĩ, Tiến sĩ, IELTS 8.0...

        [StringLength(100)]
        [Display(Name = "Chuyên môn")]
        public string? ChuyenMon { get; set; } // Tiếng Anh Giao Tiếp, IELTS, TOEIC, Tiếng Nhật, Tiếng Hàn...

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Trạng thái công tác")]
        public bool TrangThai { get; set; } = true; // true: Đang giảng dạy, false: Tạm nghỉ

        // Navigation Property: 1 giảng viên có thể phụ trách nhiều lớp học
        public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();
    }
}
