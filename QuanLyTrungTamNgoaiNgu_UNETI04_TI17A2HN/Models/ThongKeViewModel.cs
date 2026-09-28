// Họ và tên: Lê Đăng Duy
// Mã sinh viên: 23103100089
// Nội dung thực hiện: Module 4 - ViewModel các thống kê (số lượng, học phí)

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models.ViewModels
{
    public class ThongKeSoLuongItem
    {
        public string Ten { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    public class ThongKeLopItem
    {
        public string TenLop { get; set; } = string.Empty;
        public int TongDangKy { get; set; }
        public int DaXacNhan { get; set; }
    }

    public class ThongKeHocPhiItem
    {
        public string Ten { get; set; } = string.Empty;
        public decimal TongHocPhi { get; set; }
    }

    public class ThongKeViewModel
    {
        public List<ThongKeSoLuongItem> SoLopTheoKhoaHoc { get; set; } = new();
        public List<ThongKeLopItem> DangKyTheoLop { get; set; } = new();
        public List<ThongKeSoLuongItem> DangKyTheoTrangThai { get; set; } = new();
        public List<ThongKeLopItem> LopNhieuDangKyNhat { get; set; } = new();
        public List<ThongKeHocPhiItem> HocPhiTheoLop { get; set; } = new();
        public List<ThongKeHocPhiItem> HocPhiTheoKhoaHoc { get; set; } = new();
    }
}