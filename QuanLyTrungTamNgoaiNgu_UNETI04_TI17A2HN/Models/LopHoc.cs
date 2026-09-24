// Họ và tên: Phạm Đức Duy - 23103100084
// Nội dung thực hiện: Module 2 - Xây dựng Entity LopHoc và Data Annotation chuẩn đề bài

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    [Table("LopHoc")]
    public class LopHoc
    {
        [Key]
        [Display(Name = "Mã lớp")]
        public int MaLop { get; set; }

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(100, ErrorMessage = "Tên lớp không vượt quá 100 ký tự")]
        [Display(Name = "Tên lớp")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        [Display(Name = "Khóa học")]
        public int MaKhoaHoc { get; set; }

        [ForeignKey(nameof(MaKhoaHoc))]
        [Display(Name = "Khóa học")]
        public virtual KhoaHoc? KhoaHoc { get; set; }

        [Required(ErrorMessage = "Ngày khai giảng không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày khai giảng")]
        public DateTime NgayKhaiGiang { get; set; }

        [Required(ErrorMessage = "Lịch học không được để trống")]
        [StringLength(100, ErrorMessage = "Lịch học không vượt quá 100 ký tự")]
        [Display(Name = "Lịch học")]
        public string LichHoc { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Phòng học không được để trống")]
        [StringLength(50, ErrorMessage = "Phòng học không vượt quá 50 ký tự")]
        [Display(Name = "Phòng học")]
        public string PhongHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lượng học viên tối đa không được để trống")]
        [Range(1, 500, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        [Display(Name = "Số lượng tối đa")]
        public int SoLuongToiDa { get; set; }

        [Required(ErrorMessage = "Học phí không được để trống")]
        [Range(1, double.MaxValue, ErrorMessage = "Học phí phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Học phí")]
        public decimal HocPhi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái lớp")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Sắp mở";
       
        public virtual ICollection<DangKyHoc>? DangKyHocs { get; set; }
    }
}
