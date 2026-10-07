// Họ và tên: Phạm Văn Công
// Mã sinh viên: 23103100115
// Nội dung thực hiện: Khởi tạo dữ liệu mẫu (Seed Data) cho ApplicationDbContext

using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();
            if (context.TaiKhoans.Any())
            {
                return;
            }

            // 1. Tạo tài khoản mẫu
            var admin1 = new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhau = "123456",
                HoTen = "Quản Trị Viên Hùng",
                Email = "admin@uneti.edu.vn",
                VaiTro = "Admin",
                TrangThai = true
            };

            var studentTk1 = new TaiKhoan
            {
                TenDangNhap = "student1",
                MatKhau = "123456",
                HoTen = "Nguyễn Văn Nam",
                Email = "nam.nv@student.uneti.edu.vn",
                VaiTro = "HocVien",
                TrangThai = true
            };

            var studentTk2 = new TaiKhoan
            {
                TenDangNhap = "student2",
                MatKhau = "123456",
                HoTen = "Trần Thị Mai",
                Email = "mai.tt@student.uneti.edu.vn",
                VaiTro = "HocVien",
                TrangThai = true
            };

            context.TaiKhoans.AddRange(admin1, studentTk1, studentTk2);
            context.SaveChanges();

            // 2. Tạo Hồ sơ học viên
            var hv1 = new HocVien
            {
                MaTaiKhoan = studentTk1.MaTaiKhoan,
                HoTen = studentTk1.HoTen,
                NgaySinh = new DateTime(2003, 5, 12),
                GioiTinh = "Nam",
                SoDienThoai = "0987654321",
                Email = studentTk1.Email,
                DiaChi = "Hai Bà Trưng, Hà Nội"
            };

            var hv2 = new HocVien
            {
                MaTaiKhoan = studentTk2.MaTaiKhoan,
                HoTen = studentTk2.HoTen,
                NgaySinh = new DateTime(2004, 8, 20),
                GioiTinh = "Nữ",
                SoDienThoai = "0912345678",
                Email = studentTk2.Email,
                DiaChi = "Hoàng Mai, Hà Nội"
            };

            context.HocViens.AddRange(hv1, hv2);
            context.SaveChanges();

            // 3. Tạo Khóa học mẫu
            var kh1 = new KhoaHoc
            {
                TenKhoaHoc = "Tiếng Anh Giao Tiếp Căn Bản",
                NgoaiNgu = "Tiếng Anh",
                CapDo = "Sơ cấp",
                HocPhi = 2500000m,
                MoTa = "Chương trình dành cho người bắt đầu, cải thiện phản xạ phát âm và giao tiếp hằng ngày."
            };

            var kh2 = new KhoaHoc
            {
                TenKhoaHoc = "Luyện Thi IELTS 6.5+",
                NgoaiNgu = "Tiếng Anh",
                CapDo = "Trung cấp",
                HocPhi = 5800000m,
                MoTa = "Khóa học chiến thuật chuyên sâu 4 kỹ năng Nghe, Nói, Đọc, Viết đạt mục tiêu IELTS 6.5."
            };

            var kh3 = new KhoaHoc
            {
                TenKhoaHoc = "Tiếng Trung HSK 3 Chuẩn Quốc Tế",
                NgoaiNgu = "Tiếng Trung",
                CapDo = "Trung cấp",
                HocPhi = 3200000m,
                MoTa = "Trang bị 600 từ vựng HSK 3, viết chữ Hán và giao tiếp tự tin cùng giáo viên bản ngữ."
            };

            var kh4 = new KhoaHoc
            {
                TenKhoaHoc = "Tiếng Nhật JLPT N5 Bứt Phá",
                NgoaiNgu = "Tiếng Nhật",
                CapDo = "Sơ cấp",
                HocPhi = 2900000m,
                MoTa = "Học bảng chữ cái Hiragana, Katakana và 800 từ vựng căn bản vượt qua kỳ thi N5."
            };

            context.KhoaHocs.AddRange(kh1, kh2, kh3, kh4);
            context.SaveChanges();

            // 4. Tạo Lớp học mẫu
            var lop1 = new LopHoc
            {
                MaKhoaHoc = kh1.MaKhoaHoc,
                TenLop = "ENG-BASIC-01",
                NgayKhaiGiang = DateTime.Now.AddDays(10),
                LichHoc = "Tối Thứ 2, 4, 6 (18h30 - 20h30)",
                PhongHoc = "P.402A - Cơ sở Minh Khai",
                SoLuongToiDa = 25,
                HocPhi = 2500000m,
                TrangThai = "Đang tuyển sinh"
            };

            var lop2 = new LopHoc
            {
                MaKhoaHoc = kh2.MaKhoaHoc,
                TenLop = "IELTS-65-K02",
                NgayKhaiGiang = DateTime.Now.AddDays(15),
                LichHoc = "Chiều Thứ 7, Chủ Nhật (14h00 - 17h00)",
                PhongHoc = "P.501B - Cơ sở Lĩnh Nam",
                SoLuongToiDa = 20,
                HocPhi = 5800000m,
                TrangThai = "Đang tuyển sinh"
            };

            var lop3 = new LopHoc
            {
                MaKhoaHoc = kh3.MaKhoaHoc,
                TenLop = "HSK3-CHINESE-01",
                NgayKhaiGiang = DateTime.Now.AddDays(5),
                LichHoc = "Tối Thứ 3, 5, 7 (19h00 - 21h00)",
                PhongHoc = "P.303A - Cơ sở Minh Khai",
                SoLuongToiDa = 20,
                HocPhi = 3200000m,
                TrangThai = "Đang tuyển sinh"
            };

            context.LopHocs.AddRange(lop1, lop2, lop3);
            context.SaveChanges();

            // 5. Tạo Đơn đăng ký học mẫu
            var dk1 = new DangKyHoc
            {
                MaHocVien = hv1.MaHocVien,
                MaLop = lop1.MaLop,
                NgayDangKy = DateTime.Now.AddDays(-2),
                HocPhi = lop1.HocPhi,
                TrangThai = "Đã xác nhận"
            };

            var dk2 = new DangKyHoc
            {
                MaHocVien = hv2.MaHocVien,
                MaLop = lop2.MaLop,
                NgayDangKy = DateTime.Now.AddDays(-1),
                HocPhi = lop2.HocPhi,
                TrangThai = "Chờ xác nhận"
            };

            context.DangKyHocs.AddRange(dk1, dk2);
            context.SaveChanges();
        }
    }
}
