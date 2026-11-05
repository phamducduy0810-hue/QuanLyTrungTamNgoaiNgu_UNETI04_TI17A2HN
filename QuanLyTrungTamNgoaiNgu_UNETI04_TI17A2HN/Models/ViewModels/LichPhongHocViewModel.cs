// Họ và tên: Phạm Đức Duy - 23103100084 & Nhóm UNETI04
// Nội dung thực hiện: ViewModel kiểm tra trùng phòng học & ca học, hiển thị ma trận lịch phòng học

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models.ViewModels
{
    public class CanhBaoTrungLichItem
    {
        public string PhongHoc { get; set; } = string.Empty;
        public LopHoc Lop1 { get; set; } = null!;
        public LopHoc Lop2 { get; set; } = null!;
        public string LyDoTrung { get; set; } = string.Empty;
    }

    public class LichPhongHocViewModel
    {
        public List<string> DanhSachPhongHoc { get; set; } = new();
        public List<string> DanhSachCaHoc { get; set; } = new();
        public List<LopHoc> TatCaLopHoatDong { get; set; } = new();
        public List<CanhBaoTrungLichItem> DanhSachTrungLich { get; set; } = new();
        public string? PhongHocLoc { get; set; }
        public string? CaHocLoc { get; set; }
        public int TongSoLopDangHoc { get; set; }
        public int TongSoPhongSuDung { get; set; }
    }
}
