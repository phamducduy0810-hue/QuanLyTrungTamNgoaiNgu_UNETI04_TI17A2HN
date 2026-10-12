using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Tự động tạo cơ sở dữ liệu nếu chưa có
            context.Database.EnsureCreated();

            // Nếu đã có lớp học thì không seed lại
            if (context.LopHocs.Any())
            {
                return;
            }

            // 1. Seed Khóa học (Tối thiểu 5 khóa học)
            var khoaHocs = new List<KhoaHoc>
            {
                new KhoaHoc
                {
                    TenKhoaHoc = "Tiếng Anh Giao Tiếp Cơ Bản (A1)",
                    NgoaiNgu = "Tiếng Anh",
                    CapDo = "A1",
                    HocPhi = 2500000,
                    SoBuoi = 24,
                    MoTa = "Khóa học dành cho người mất gốc, rèn luyện phát âm chuẩn IPA và phản xạ giao tiếp cơ bản hàng ngày.",
                    TrangThai = true
                },
                new KhoaHoc
                {
                    TenKhoaHoc = "Luyện Thi IELTS Chuyên Sâu 6.5+",
                    NgoaiNgu = "Tiếng Anh",
                    CapDo = "B2 - C1",
                    HocPhi = 5500000,
                    SoBuoi = 36,
                    MoTa = "Rèn luyện 4 kỹ năng Nghe - Nói - Đọc - Viết theo format đề thi IELTS quốc tế chuẩn Cambridge.",
                    TrangThai = true
                },
                new KhoaHoc
                {
                    TenKhoaHoc = "Tiếng Trung Sơ Cấp (HSK 1 - 2)",
                    NgoaiNgu = "Tiếng Trung",
                    CapDo = "HSK 2",
                    HocPhi = 2800000,
                    SoBuoi = 30,
                    MoTa = "Khóa học nhập môn tiếng Trung: bảng chữ cái Pinyin, quy tắc viết chữ Hán và các hội thoại sinh hoạt thông dụng.",
                    TrangThai = true
                },
                new KhoaHoc
                {
                    TenKhoaHoc = "Tiếng Trung Thương Mại (HSK 4 - 5)",
                    NgoaiNgu = "Tiếng Trung",
                    CapDo = "HSK 4",
                    HocPhi = 4200000,
                    SoBuoi = 32,
                    MoTa = "Khóa học chuyên sâu từ vựng, ngữ pháp thương mại, đàm phán hợp đồng và giao tiếp công sở tiếng Trung.",
                    TrangThai = true
                },
                new KhoaHoc
                {
                    TenKhoaHoc = "Tiếng Nhật Sơ Cấp N5 Cấp Tốc",
                    NgoaiNgu = "Tiếng Nhật",
                    CapDo = "N5",
                    HocPhi = 3200000,
                    SoBuoi = 36,
                    MoTa = "Làm quen với 2 bảng chữ cái Hiragana, Katakana, chữ Hán cơ bản và ngữ pháp sơ cấp chuẩn bị cho kỳ thi JLPT N5.",
                    TrangThai = true
                },
                new KhoaHoc
                {
                    TenKhoaHoc = "Tiếng Hàn Giao Tiếp & TOPIK I",
                    NgoaiNgu = "Tiếng Hàn",
                    CapDo = "TOPIK 1",
                    HocPhi = 2900000,
                    SoBuoi = 28,
                    MoTa = "Học bảng chữ cái Hangul, cách phát âm, ngữ pháp và từ vựng chuẩn bị cho kỳ thi TOPIK level 1 và 2.",
                    TrangThai = true
                }
            };

            context.KhoaHocs.AddRange(khoaHocs);
            context.SaveChanges();

            // 2. Seed Lớp học (Tối thiểu 10 lớp học đa dạng để test lọc, sắp xếp, phân trang)
            var lopHocs = new List<LopHoc>
            {
                new LopHoc
                {
                    TenLop = "TAGT-A1-K01",
                    MaKhoaHoc = khoaHocs[0].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(5),
                    LichHoc = "T2 - T4 - T6 (18:00 - 20:00)",
                    PhongHoc = "Phòng A101",
                    SoLuongToiDa = 20,
                    HocPhi = 2500000,
                    TrangThai = "Đang tuyển sinh"
                },
                new LopHoc
                {
                    TenLop = "TAGT-A1-K02",
                    MaKhoaHoc = khoaHocs[0].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(15),
                    LichHoc = "T3 - T5 - T7 (19:30 - 21:30)",
                    PhongHoc = "Phòng A102",
                    SoLuongToiDa = 25,
                    HocPhi = 2500000,
                    TrangThai = "Sắp mở"
                },
                new LopHoc
                {
                    TenLop = "IELTS-65-K01",
                    MaKhoaHoc = khoaHocs[1].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(10),
                    LichHoc = "T2 - T4 - T6 (19:30 - 21:30)",
                    PhongHoc = "Phòng B201",
                    SoLuongToiDa = 15,
                    HocPhi = 5500000,
                    TrangThai = "Đang tuyển sinh"
                },
                new LopHoc
                {
                    TenLop = "IELTS-65-K02",
                    MaKhoaHoc = khoaHocs[1].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(-20),
                    LichHoc = "T7 - CN (14:00 - 17:00)",
                    PhongHoc = "Phòng B202",
                    SoLuongToiDa = 15,
                    HocPhi = 5500000,
                    TrangThai = "Đang học"
                },
                new LopHoc
                {
                    TenLop = "TT-HSK2-K01",
                    MaKhoaHoc = khoaHocs[2].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(7),
                    LichHoc = "T3 - T5 - T7 (18:00 - 20:00)",
                    PhongHoc = "Phòng C301",
                    SoLuongToiDa = 20,
                    HocPhi = 2800000,
                    TrangThai = "Đang tuyển sinh"
                },
                new LopHoc
                {
                    TenLop = "TT-HSK2-K02",
                    MaKhoaHoc = khoaHocs[2].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(-60),
                    LichHoc = "T2 - T4 - T6 (18:00 - 20:00)",
                    PhongHoc = "Phòng C302",
                    SoLuongToiDa = 18,
                    HocPhi = 2800000,
                    TrangThai = "Đã kết thúc"
                },
                new LopHoc
                {
                    TenLop = "TT-TM-K01",
                    MaKhoaHoc = khoaHocs[3].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(20),
                    LichHoc = "T7 - CN (08:30 - 11:30)",
                    PhongHoc = "Phòng C303",
                    SoLuongToiDa = 15,
                    HocPhi = 4200000,
                    TrangThai = "Sắp mở"
                },
                new LopHoc
                {
                    TenLop = "TN-N5-K01",
                    MaKhoaHoc = khoaHocs[4].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(3),
                    LichHoc = "T2 - T4 - T6 (18:00 - 20:00)",
                    PhongHoc = "Phòng D401",
                    SoLuongToiDa = 20,
                    HocPhi = 3200000,
                    TrangThai = "Đang tuyển sinh"
                },
                new LopHoc
                {
                    TenLop = "TN-N5-K02",
                    MaKhoaHoc = khoaHocs[4].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(-15),
                    LichHoc = "T3 - T5 - T7 (19:30 - 21:30)",
                    PhongHoc = "Phòng D402",
                    SoLuongToiDa = 20,
                    HocPhi = 3200000,
                    TrangThai = "Đang học"
                },
                new LopHoc
                {
                    TenLop = "TH-TOPIK-K01",
                    MaKhoaHoc = khoaHocs[5].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(8),
                    LichHoc = "T2 - T4 - T6 (19:30 - 21:30)",
                    PhongHoc = "Phòng E501",
                    SoLuongToiDa = 22,
                    HocPhi = 2900000,
                    TrangThai = "Đang tuyển sinh"
                },
                new LopHoc
                {
                    TenLop = "TH-TOPIK-K02",
                    MaKhoaHoc = khoaHocs[5].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(25),
                    LichHoc = "T7 - CN (18:00 - 21:00)",
                    PhongHoc = "Phòng E502",
                    SoLuongToiDa = 20,
                    HocPhi = 2900000,
                    TrangThai = "Sắp mở"
                },
                new LopHoc
                {
                    TenLop = "TAGT-A1-K03",
                    MaKhoaHoc = khoaHocs[0].MaKhoaHoc,
                    NgayKhaiGiang = DateTime.Today.AddDays(-45),
                    LichHoc = "T2 - T4 - T6 (19:30 - 21:30)",
                    PhongHoc = "Phòng A103",
                    SoLuongToiDa = 20,
                    HocPhi = 2500000,
                    TrangThai = "Đã kết thúc"
                }
            };

            context.LopHocs.AddRange(lopHocs);
            context.SaveChanges();

            // 3. Seed Tài khoản Admin và Học viên
            var adminTk = new TaiKhoan
            {
                TenDangNhap = "admin",
                MatKhau = "admin123",
                HoTen = "Quản Trị Viên Trung Tâm",
                Email = "admin@uneti.edu.vn",
                VaiTro = "Admin",
                TrangThai = true
            };
            var adminTk2 = new TaiKhoan
            {
                TenDangNhap = "admin2",
                MatKhau = "admin123",
                HoTen = "Phạm Đức Duy (Admin)",
                Email = "duy.admin@uneti.edu.vn",
                VaiTro = "Admin",
                TrangThai = true
            };
            context.TaiKhoans.AddRange(adminTk, adminTk2);
            context.SaveChanges();

            // Tạo 15 tài khoản học viên và hồ sơ học viên tương ứng
            var studentNames = new[]
            {
                ("hocvien01", "Nguyễn Văn An", "Nam", "0912345601", "an.nv@gmail.com", "Hà Nội"),
                ("hocvien02", "Trần Thị Bình", "Nữ", "0912345602", "binh.tt@gmail.com", "Bắc Ninh"),
                ("hocvien03", "Lê Hoàng Cường", "Nam", "0912345603", "cuong.lh@gmail.com", "Hải Phòng"),
                ("hocvien04", "Phạm Thùy Dung", "Nữ", "0912345604", "dung.pt@gmail.com", "Nam Định"),
                ("hocvien05", "Đỗ Minh Đức", "Nam", "0912345605", "duc.dm@gmail.com", "Thái Bình"),
                ("hocvien06", "Vũ Mai Hoa", "Nữ", "0912345606", "hoa.vm@gmail.com", "Hà Nam"),
                ("hocvien07", "Hoàng Tuấn Kiên", "Nam", "0912345607", "kien.ht@gmail.com", "Hưng Yên"),
                ("hocvien08", "Ngô Bích Lan", "Nữ", "0912345608", "lan.nb@gmail.com", "Vĩnh Phúc"),
                ("hocvien09", "Đặng Quang Minh", "Nam", "0912345609", "minh.dq@gmail.com", "Hà Nội"),
                ("hocvien10", "Bùi Kim Ngân", "Nữ", "0912345610", "ngan.bk@gmail.com", "Hải Dương"),
                ("hocvien11", "Dương Nhật Nam", "Nam", "0912345611", "nam.dn@gmail.com", "Ninh Bình"),
                ("hocvien12", "Lý Thanh Phương", "Nữ", "0912345612", "phuong.lt@gmail.com", "Phú Thọ"),
                ("hocvien13", "Tạ Hồng Quân", "Nam", "0912345613", "quan.th@gmail.com", "Thái Nguyên"),
                ("hocvien14", "Đinh Thu Thảo", "Nữ", "0912345614", "thao.dt@gmail.com", "Hà Nội"),
                ("hocvien15", "Vương Quốc Việt", "Nam", "0912345615", "viet.vq@gmail.com", "Bắc Giang")
            };

            var hocViens = new List<HocVien>();
            int birthYear = 2002;
            foreach (var s in studentNames)
            {
                var tk = new TaiKhoan
                {
                    TenDangNhap = s.Item1,
                    MatKhau = "123456",
                    HoTen = s.Item2,
                    Email = s.Item5,
                    VaiTro = "HocVien",
                    TrangThai = true
                };
                context.TaiKhoans.Add(tk);
                context.SaveChanges();

                var hv = new HocVien
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    HoTen = s.Item2,
                    NgaySinh = new DateTime(birthYear, (hocViens.Count % 12) + 1, 15),
                    GioiTinh = s.Item3,
                    SoDienThoai = s.Item4,
                    Email = s.Item5,
                    DiaChi = s.Item6
                };
                hocViens.Add(hv);
            }

            context.HocViens.AddRange(hocViens);
            context.SaveChanges();

            // 4. Seed Đăng ký học (Tối thiểu 15-20 lượt đăng ký)
            var dangKyList = new List<DangKyHoc>
            {
                new DangKyHoc { MaHocVien = hocViens[0].MaHocVien, MaLop = lopHocs[0].MaLop, NgayDangKy = DateTime.Today.AddDays(-5), HocPhi = lopHocs[0].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[1].MaHocVien, MaLop = lopHocs[0].MaLop, NgayDangKy = DateTime.Today.AddDays(-4), HocPhi = lopHocs[0].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[2].MaHocVien, MaLop = lopHocs[0].MaLop, NgayDangKy = DateTime.Today.AddDays(-3), HocPhi = lopHocs[0].HocPhi, TrangThai = "Chờ xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[3].MaHocVien, MaLop = lopHocs[2].MaLop, NgayDangKy = DateTime.Today.AddDays(-4), HocPhi = lopHocs[2].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[4].MaHocVien, MaLop = lopHocs[2].MaLop, NgayDangKy = DateTime.Today.AddDays(-2), HocPhi = lopHocs[2].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[5].MaHocVien, MaLop = lopHocs[2].MaLop, NgayDangKy = DateTime.Today.AddDays(-1), HocPhi = lopHocs[2].HocPhi, TrangThai = "Chờ xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[6].MaHocVien, MaLop = lopHocs[4].MaLop, NgayDangKy = DateTime.Today.AddDays(-3), HocPhi = lopHocs[4].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[7].MaHocVien, MaLop = lopHocs[4].MaLop, NgayDangKy = DateTime.Today.AddDays(-2), HocPhi = lopHocs[4].HocPhi, TrangThai = "Chờ xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[8].MaHocVien, MaLop = lopHocs[7].MaLop, NgayDangKy = DateTime.Today.AddDays(-2), HocPhi = lopHocs[7].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[9].MaHocVien, MaLop = lopHocs[7].MaLop, NgayDangKy = DateTime.Today.AddDays(-1), HocPhi = lopHocs[7].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[10].MaHocVien, MaLop = lopHocs[9].MaLop, NgayDangKy = DateTime.Today.AddDays(-3), HocPhi = lopHocs[9].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[11].MaHocVien, MaLop = lopHocs[9].MaLop, NgayDangKy = DateTime.Today.AddDays(-1), HocPhi = lopHocs[9].HocPhi, TrangThai = "Chờ xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[12].MaHocVien, MaLop = lopHocs[0].MaLop, NgayDangKy = DateTime.Today.AddDays(-2), HocPhi = lopHocs[0].HocPhi, TrangThai = "Đã hủy" },
                new DangKyHoc { MaHocVien = hocViens[13].MaHocVien, MaLop = lopHocs[3].MaLop, NgayDangKy = DateTime.Today.AddDays(-25), HocPhi = lopHocs[3].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[14].MaHocVien, MaLop = lopHocs[8].MaLop, NgayDangKy = DateTime.Today.AddDays(-20), HocPhi = lopHocs[8].HocPhi, TrangThai = "Đã xác nhận" },
                new DangKyHoc { MaHocVien = hocViens[0].MaHocVien, MaLop = lopHocs[7].MaLop, NgayDangKy = DateTime.Today.AddDays(-1), HocPhi = lopHocs[7].HocPhi, TrangThai = "Chờ xác nhận" }
            };

            context.DangKyHocs.AddRange(dangKyList);
            context.SaveChanges();
        }
    }
}
