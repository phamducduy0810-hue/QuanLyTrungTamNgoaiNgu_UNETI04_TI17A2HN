// Họ và tên: Phạm Văn Công 
// Mã sinh viên: 23103100115
// Nội dung thực hiện: Module 1 - Entity KhoaHoc

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    [Table("KhoaHoc")]
    public class KhoaHoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã khóa học")]
        public int MaKhoaHoc { get; set; }

        [Required(ErrorMessage = "Tên khóa học không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khóa học tối đa 100 ký tự")]
        [Display(Name = "Tên khóa học")]
        public string TenKhoaHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngoại ngữ không được để trống")]
        [StringLength(50, ErrorMessage = "Ngoại ngữ tối đa 50 ký tự")]
        [Display(Name = "Ngoại ngữ")]
        public string NgoaiNgu { get; set; } = string.Empty; // Tiếng Anh, Tiếng Trung, Tiếng Nhật, Tiếng Hàn...

        [Required(ErrorMessage = "Cấp độ không được để trống")]
        [StringLength(50, ErrorMessage = "Cấp độ tối đa 50 ký tự")]
        [Display(Name = "Cấp độ")]
        public string CapDo { get; set; } = string.Empty; // Sơ cấp, Trung cấp, Cao cấp, A1, B1, HSK3, TOPIK...

        [Required(ErrorMessage = "Học phí không được để trống")]
        [Range(1, 100000000, ErrorMessage = "Học phí phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Học phí (VNĐ)")]
        public decimal HocPhi { get; set; }

        [Required(ErrorMessage = "Số buổi không được để trống")]
        [Range(1, 365, ErrorMessage = "Số buổi phải lớn hơn 0")]
        [Display(Name = "Số buổi học")]
        public int SoBuoi { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true: Đang mở, false: Tạm dừng

        // Navigation Property 1-n với LopHoc
        public virtual ICollection<LopHoc>? LopHocs { get; set; }
    }
}
