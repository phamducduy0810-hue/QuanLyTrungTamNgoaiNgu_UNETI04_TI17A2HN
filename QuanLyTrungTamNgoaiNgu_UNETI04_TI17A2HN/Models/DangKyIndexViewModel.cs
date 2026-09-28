// Họ và tên: Lê Đăng Duy
// Mã sinh viên: 23103100089
// Nội dung thực hiện: Module 4 - ViewModel quản lý đăng ký (tìm kiếm, lọc) và hằng số trạng thái đăng ký

using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models
{
    // Các giá trị trạng thái đăng ký dùng chung, tránh gõ sai chuỗi ở nhiều nơi
    public static class TrangThaiDangKy
    {
        public const string ChoXacNhan = "Chờ xác nhận";
        public const string DaXacNhan = "Đã xác nhận";
        public const string DaHuy = "Đã hủy";

        public static readonly string[] TatCa = { ChoXacNhan, DaXacNhan, DaHuy };
    }

    public class DangKyIndexViewModel
    {
        public List<DangKyHoc> DanhSach { get; set; } = new();

        // Điều kiện tìm kiếm / lọc (giữ lại để hiển thị lại trên form)
        public string? TuKhoa { get; set; }       // tên học viên
        public int? MaLop { get; set; }           // lọc theo lớp
        public string? TrangThai { get; set; }    // lọc theo trạng thái

        // Dữ liệu cho các ô select
        public List<SelectListItem> DanhSachLop { get; set; } = new();
        public List<string> DanhSachTrangThai { get; set; } = new();
    }
}