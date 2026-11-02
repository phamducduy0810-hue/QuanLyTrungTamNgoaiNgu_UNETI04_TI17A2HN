// Họ và tên: Diêm Đức Hùng
// Mã sinh viên: 23103100099
// Nội dung thực hiện: Module 3 - ViewModel quản lý phân công lớp học cho giảng viên 

using System.Collections.Generic;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    public class PhanCongLopViewModel
    {
        public GiangVien? GiangVienChon { get; set; }
        public List<LopHoc> DanhSachLop { get; set; } = new List<LopHoc>();
        public List<GiangVien> DanhSachGiangVien { get; set; } = new List<GiangVien>();
        public List<KhoaHoc> DanhSachKhoaHoc { get; set; } = new List<KhoaHoc>();

        // Bộ lọc và tìm kiếm
        public string? TuKhoa { get; set; }
        public int? MaKhoaHoc { get; set; }
        public int? MaGiangVien { get; set; }
        public string? TrangThaiPhanCong { get; set; } // "DaPhanCong", "ChuaPhanCong", "TatCa"
    }
}
