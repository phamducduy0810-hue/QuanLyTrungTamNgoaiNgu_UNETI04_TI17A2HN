// Họ và tên: Lê Đăng Duy
// Mã sinh viên: 23103100089
// Nội dung thực hiện: Module 4 - Dashboard và thống kê bằng LINQ (Count, Sum, GroupBy, OrderByDescending)
// Bổ sung: Chức năng Xuất / Nhập Excel báo cáo thống kê chuyên nghiệp bằng ClosedXML

using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models.ViewModels;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Admin ở phía Server
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro))
            {
                context.Result = RedirectToAction("Login", "TaiKhoan");
            }
            else if (vaiTro != "Admin")
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                context.Result = RedirectToAction("Index", "Home");
            }
            base.OnActionExecuting(context);
        }

        // GET: /ThongKe/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var vm = new DashboardViewModel
            {
                TongKhoaHoc = await _context.KhoaHocs.CountAsync(),
                TongLopHoc = await _context.LopHocs.CountAsync(),
                TongHocVien = await _context.HocViens.CountAsync(),
                TongLuotDangKy = await _context.DangKyHocs.CountAsync(),
                DangKyChoXacNhan = await _context.DangKyHocs
                    .CountAsync(d => d.TrangThai == TrangThaiDangKy.ChoXacNhan),
                DangKyDaXacNhan = await _context.DangKyHocs
                    .CountAsync(d => d.TrangThai == TrangThaiDangKy.DaXacNhan),
                LopDangTuyenSinh = await _context.LopHocs
                    .CountAsync(l => l.TrangThai == "Đang tuyển sinh")
            };

            return View(vm);
        }

        // GET: /ThongKe
        public async Task<IActionResult> Index()
        {
            var vm = new ThongKeViewModel();

            // 1. Số lớp theo từng khóa học (khóa chưa có lớp vẫn hiện với số 0)
            vm.SoLopTheoKhoaHoc = await _context.KhoaHocs
                .Select(k => new ThongKeSoLuongItem
                {
                    Ten = k.TenKhoaHoc,
                    SoLuong = k.LopHocs!.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.Ten)
                .ToListAsync();

            // 2. Số học viên đăng ký theo từng lớp (tổng và đã xác nhận)
            vm.DangKyTheoLop = await _context.LopHocs
                .Select(l => new ThongKeLopItem
                {
                    TenLop = l.TenLop,
                    TongDangKy = l.DangKyHocs!.Count(),
                    DaXacNhan = l.DangKyHocs!.Count(d => d.TrangThai == TrangThaiDangKy.DaXacNhan)
                })
                .OrderByDescending(x => x.TongDangKy)
                .ThenBy(x => x.TenLop)
                .ToListAsync();

            // 3. Số đăng ký theo từng trạng thái
            var theoTrangThai = await _context.DangKyHocs
                .GroupBy(d => d.TrangThai)
                .Select(g => new ThongKeSoLuongItem { Ten = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            // Bổ sung trạng thái chưa có đăng ký nào để luôn hiển thị đủ 3 dòng
            foreach (var tt in TrangThaiDangKy.TatCa)
            {
                if (!theoTrangThai.Any(x => x.Ten == tt))
                    theoTrangThai.Add(new ThongKeSoLuongItem { Ten = tt, SoLuong = 0 });
            }
            vm.DangKyTheoTrangThai = theoTrangThai
                .OrderBy(x => Array.IndexOf(TrangThaiDangKy.TatCa, x.Ten))
                .ToList();

            // 4. Lớp có nhiều đăng ký nhất (lấy tất cả lớp bằng điểm cao nhất, nếu có)
            int max = vm.DangKyTheoLop.Any() ? vm.DangKyTheoLop.Max(x => x.TongDangKy) : 0;
            vm.LopNhieuDangKyNhat = max > 0
                ? vm.DangKyTheoLop.Where(x => x.TongDangKy == max).ToList()
                : new List<ThongKeLopItem>();

            // 5. Tổng học phí theo lớp - CHỈ tính đăng ký "Đã xác nhận"
            vm.HocPhiTheoLop = await _context.DangKyHocs
                .Where(d => d.TrangThai == TrangThaiDangKy.DaXacNhan)
                .GroupBy(d => new { MaLop = d.MaLop, TenLop = d.LopHoc!.TenLop })
                .Select(g => new ThongKeHocPhiItem
                {
                    Ten = g.Key.TenLop,
                    TongHocPhi = g.Sum(d => d.HocPhi)
                })
                .OrderByDescending(x => x.TongHocPhi)
                .ToListAsync();

            // 6. Tổng học phí theo khóa học - CHỈ tính đăng ký "Đã xác nhận"
            vm.HocPhiTheoKhoaHoc = await _context.DangKyHocs
                .Where(d => d.TrangThai == TrangThaiDangKy.DaXacNhan)
                .GroupBy(d => new
                {
                    MaKhoaHoc = d.LopHoc!.MaKhoaHoc,
                    TenKhoaHoc = d.LopHoc!.KhoaHoc!.TenKhoaHoc
                })
                .Select(g => new ThongKeHocPhiItem
                {
                    Ten = g.Key.TenKhoaHoc,
                    TongHocPhi = g.Sum(d => d.HocPhi)
                })
                .OrderByDescending(x => x.TongHocPhi)
                .ToListAsync();

            // Nhận kết quả import từ TempData (nếu vừa thực hiện nhập Excel)
            if (TempData["ImportResultJson"] is string importJson)
            {
                try
                {
                    vm.KetQuaImport = JsonSerializer.Deserialize<ImportThongKeResult>(importJson);
                }
                catch
                {
                    // Ignore JSON parse error if any
                }
            }

            return View(vm);
        }

        // =========================================================================
        // CHỨC NĂNG 1: XUẤT BÁO CÁO THỐNG KÊ RA FILE EXCEL (.XLSX)
        // =========================================================================
        public async Task<IActionResult> ExportExcel()
        {
            // Lấy toàn bộ dữ liệu thống kê
            var tongKhoaHoc = await _context.KhoaHocs.CountAsync();
            var tongLopHoc = await _context.LopHocs.CountAsync();
            var tongHocVien = await _context.HocViens.CountAsync();
            var tongLuotDangKy = await _context.DangKyHocs.CountAsync();
            var dangKyChoXacNhan = await _context.DangKyHocs.CountAsync(d => d.TrangThai == TrangThaiDangKy.ChoXacNhan);
            var dangKyDaXacNhan = await _context.DangKyHocs.CountAsync(d => d.TrangThai == TrangThaiDangKy.DaXacNhan);
            var dangKyDaHuy = await _context.DangKyHocs.CountAsync(d => d.TrangThai == TrangThaiDangKy.DaHuy);
            var lopDangTuyenSinh = await _context.LopHocs.CountAsync(l => l.TrangThai == "Đang tuyển sinh");

            var dsKhoaHoc = await _context.KhoaHocs
                .Include(k => k.LopHocs)
                .ToListAsync();

            var dsLopHoc = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Include(l => l.DangKyHocs)
                .ToListAsync();

            var dsDangKy = await _context.DangKyHocs
                .Include(d => d.HocVien)
                .Include(d => d.LopHoc)
                    .ThenInclude(l => l!.KhoaHoc)
                .OrderByDescending(d => d.NgayDangKy)
                .ToListAsync();

            var tongHocPhiDaXacNhan = dsDangKy
                .Where(d => d.TrangThai == TrangThaiDangKy.DaXacNhan)
                .Sum(d => d.HocPhi);

            var nguoiXuat = HttpContext.Session.GetString("HoTen") ?? "Ban Quản trị UNETI";
            var thoiGianXuat = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            using var workbook = new XLWorkbook();

            // Định dạng màu chuẩn nhận diện UNETI
            var unetiPrimaryColor = XLColor.FromArgb(30, 58, 138); // #1E3A8A
            var unetiSecondaryColor = XLColor.FromArgb(2, 132, 199); // #0284C7
            var tableHeaderColor = XLColor.FromArgb(241, 245, 249);
            var borderColor = XLColor.FromArgb(203, 213, 225);

            // ---------------------------------------------------------------------
            // SHEET 1: TỔNG QUAN CHỈ SỐ
            // ---------------------------------------------------------------------
            var ws1 = workbook.Worksheets.Add("Tổng Quan");
            ws1.ShowGridLines = true;

            // Tiêu đề banner
            ws1.Range("A1:G1").Merge().Value = "TRUNG TÂM NGOẠI NGỮ UNETI - DỰ ÁN UNETI04 (TI17A2HN)";
            ws1.Cell("A1").Style.Font.Bold = true;
            ws1.Cell("A1").Style.Font.FontSize = 14;
            ws1.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws1.Cell("A1").Style.Fill.BackgroundColor = unetiPrimaryColor;
            ws1.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws1.Row(1).Height = 28;

            ws1.Range("A2:G2").Merge().Value = "BÁO CÁO THỐNG KÊ TỔNG QUAN HOẠT ĐỘNG & DOANH THU";
            ws1.Cell("A2").Style.Font.Bold = true;
            ws1.Cell("A2").Style.Font.FontSize = 13;
            ws1.Cell("A2").Style.Font.FontColor = XLColor.White;
            ws1.Cell("A2").Style.Fill.BackgroundColor = unetiSecondaryColor;
            ws1.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws1.Row(2).Height = 24;

            ws1.Cell("A3").Value = $"Thời gian xuất báo cáo: {thoiGianXuat} | Người lập: {nguoiXuat}";
            ws1.Cell("A3").Style.Font.Italic = true;
            ws1.Cell("A3").Style.Font.FontColor = XLColor.FromHtml("#475569");
            ws1.Range("A3:G3").Merge();

            // Phần 1: Quy mô trung tâm
            int r1 = 5;
            ws1.Cell(r1, 1).Value = "I. QUY MÔ TRUNG TÂM & LỚP HỌC";
            ws1.Cell(r1, 1).Style.Font.Bold = true;
            ws1.Cell(r1, 1).Style.Font.FontSize = 11;
            ws1.Cell(r1, 1).Style.Font.FontColor = unetiPrimaryColor;

            r1++;
            string[] headersQuyMo = { "STT", "Chỉ số thống kê", "Số lượng", "Đơn vị tính", "Ghi chú" };
            for (int i = 0; i < headersQuyMo.Length; i++)
            {
                var c = ws1.Cell(r1, i + 1);
                c.Value = headersQuyMo[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            var dsQuyMo = new[]
            {
                ("1", "Tổng số chương trình / khóa học", (double)tongKhoaHoc, "Khóa", "Bao gồm cả các cấp độ ngoại ngữ"),
                ("2", "Tổng số lớp học đã tạo", (double)tongLopHoc, "Lớp", "Tổng số lớp trên hệ thống"),
                ("3", "Số lớp đang tuyển sinh", (double)lopDangTuyenSinh, "Lớp", "Lớp đang nhận học viên đăng ký"),
                ("4", "Tổng số học viên", (double)tongHocVien, "Học viên", "Học viên đã mở tài khoản")
            };

            foreach (var item in dsQuyMo)
            {
                r1++;
                ws1.Cell(r1, 1).Value = item.Item1;
                ws1.Cell(r1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws1.Cell(r1, 2).Value = item.Item2;
                ws1.Cell(r1, 3).Value = item.Item3;
                ws1.Cell(r1, 3).Style.NumberFormat.Format = "#,##0";
                ws1.Cell(r1, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws1.Cell(r1, 4).Value = item.Item4;
                ws1.Cell(r1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws1.Cell(r1, 5).Value = item.Item5;

                for (int c = 1; c <= 5; c++)
                {
                    ws1.Cell(r1, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws1.Cell(r1, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            // Phần 2: Đăng ký học & Doanh thu
            r1 += 2;
            ws1.Cell(r1, 1).Value = "II. TÌNH HÌNH ĐĂNG KÝ HỌC & DOANH THU";
            ws1.Cell(r1, 1).Style.Font.Bold = true;
            ws1.Cell(r1, 1).Style.Font.FontSize = 11;
            ws1.Cell(r1, 1).Style.Font.FontColor = unetiPrimaryColor;

            r1++;
            string[] headersDangKy = { "STT", "Chỉ tiêu", "Số lượng", "Tỷ lệ (%)", "Doanh thu (VNĐ)" };
            for (int i = 0; i < headersDangKy.Length; i++)
            {
                var c = ws1.Cell(r1, i + 1);
                c.Value = headersDangKy[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(220, 252, 231);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            double pctXacNhan = tongLuotDangKy > 0 ? (double)dangKyDaXacNhan / tongLuotDangKy : 0;
            double pctCho = tongLuotDangKy > 0 ? (double)dangKyChoXacNhan / tongLuotDangKy : 0;
            double pctHuy = tongLuotDangKy > 0 ? (double)dangKyDaHuy / tongLuotDangKy : 0;

            var dsChiTieuDangKy = new[]
            {
                ("1", "Tổng số lượt đăng ký", (double)tongLuotDangKy, 1.0, 0.0),
                ("2", "Đăng ký đã xác nhận", (double)dangKyDaXacNhan, pctXacNhan, (double)tongHocPhiDaXacNhan),
                ("3", "Đăng ký chờ xác nhận", (double)dangKyChoXacNhan, pctCho, 0.0),
                ("4", "Đăng ký đã hủy", (double)dangKyDaHuy, pctHuy, 0.0)
            };

            foreach (var item in dsChiTieuDangKy)
            {
                r1++;
                ws1.Cell(r1, 1).Value = item.Item1;
                ws1.Cell(r1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws1.Cell(r1, 2).Value = item.Item2;
                ws1.Cell(r1, 3).Value = item.Item3;
                ws1.Cell(r1, 3).Style.NumberFormat.Format = "#,##0";
                ws1.Cell(r1, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws1.Cell(r1, 4).Value = item.Item4;
                ws1.Cell(r1, 4).Style.NumberFormat.Format = "0.0%";
                ws1.Cell(r1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws1.Cell(r1, 5).Value = item.Item5;
                ws1.Cell(r1, 5).Style.NumberFormat.Format = "#,##0 \"đ\"";
                ws1.Cell(r1, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                if (item.Item1 == "2")
                {
                    ws1.Row(r1).Style.Font.Bold = true;
                    ws1.Cell(r1, 5).Style.Font.FontColor = XLColor.FromArgb(22, 101, 52);
                }

                for (int c = 1; c <= 5; c++)
                {
                    ws1.Cell(r1, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws1.Cell(r1, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            ws1.Columns().AdjustToContents();

            // ---------------------------------------------------------------------
            // SHEET 2: THỐNG KÊ THEO KHÓA HỌC
            // ---------------------------------------------------------------------
            var ws2 = workbook.Worksheets.Add("KhoaHoc_DoanhThu");
            ws2.ShowGridLines = true;

            ws2.Range("A1:H1").Merge().Value = "BÁO CÁO THỐNG KÊ SỐ LỚP & DOANH THU THEO TỪNG KHÓA HỌC";
            ws2.Cell("A1").Style.Font.Bold = true;
            ws2.Cell("A1").Style.Font.FontSize = 13;
            ws2.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws2.Cell("A1").Style.Fill.BackgroundColor = unetiPrimaryColor;
            ws2.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws2.Row(1).Height = 26;

            string[] headersKH = { "STT", "Mã khóa học", "Tên khóa học", "Ngoại ngữ", "Cấp độ", "Học phí quy định (VNĐ)", "Số lớp đã mở", "Doanh thu học phí (VNĐ)" };
            for (int i = 0; i < headersKH.Length; i++)
            {
                var c = ws2.Cell(3, i + 1);
                c.Value = headersKH[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            int r2 = 3;
            int sttKH = 1;
            decimal tongDoanhThuKH = 0;
            int tongSoLopKH = 0;

            foreach (var kh in dsKhoaHoc.OrderBy(k => k.TenKhoaHoc))
            {
                r2++;
                int soLop = kh.LopHocs?.Count ?? 0;
                tongSoLopKH += soLop;

                decimal doanhThuKhoa = dsDangKy
                    .Where(d => d.LopHoc != null && d.LopHoc.MaKhoaHoc == kh.MaKhoaHoc && d.TrangThai == TrangThaiDangKy.DaXacNhan)
                    .Sum(d => d.HocPhi);
                tongDoanhThuKH += doanhThuKhoa;

                ws2.Cell(r2, 1).Value = sttKH++;
                ws2.Cell(r2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws2.Cell(r2, 2).Value = kh.MaKhoaHoc;
                ws2.Cell(r2, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws2.Cell(r2, 3).Value = kh.TenKhoaHoc;
                ws2.Cell(r2, 4).Value = kh.NgoaiNgu;
                ws2.Cell(r2, 5).Value = kh.CapDo;
                ws2.Cell(r2, 6).Value = (double)kh.HocPhi;
                ws2.Cell(r2, 6).Style.NumberFormat.Format = "#,##0 \"đ\"";
                ws2.Cell(r2, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws2.Cell(r2, 7).Value = soLop;
                ws2.Cell(r2, 7).Style.NumberFormat.Format = "#,##0";
                ws2.Cell(r2, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws2.Cell(r2, 8).Value = (double)doanhThuKhoa;
                ws2.Cell(r2, 8).Style.NumberFormat.Format = "#,##0 \"đ\"";
                ws2.Cell(r2, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                for (int c = 1; c <= 8; c++)
                {
                    ws2.Cell(r2, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws2.Cell(r2, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            // Dòng tổng cộng
            r2++;
            ws2.Cell(r2, 1).Value = "";
            ws2.Range(r2, 1, r2, 6).Merge().Value = "TỔNG CỘNG";
            ws2.Cell(r2, 1).Style.Font.Bold = true;
            ws2.Cell(r2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws2.Cell(r2, 7).Value = tongSoLopKH;
            ws2.Cell(r2, 7).Style.Font.Bold = true;
            ws2.Cell(r2, 7).Style.NumberFormat.Format = "#,##0";
            ws2.Cell(r2, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws2.Cell(r2, 8).Value = (double)tongDoanhThuKH;
            ws2.Cell(r2, 8).Style.Font.Bold = true;
            ws2.Cell(r2, 8).Style.Font.FontColor = XLColor.FromArgb(22, 101, 52);
            ws2.Cell(r2, 8).Style.NumberFormat.Format = "#,##0 \"đ\"";
            ws2.Cell(r2, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            for (int c = 1; c <= 8; c++)
            {
                ws2.Cell(r2, c).Style.Fill.BackgroundColor = XLColor.FromArgb(241, 245, 249);
                ws2.Cell(r2, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws2.Cell(r2, c).Style.Border.OutsideBorderColor = borderColor;
            }

            ws2.Columns().AdjustToContents();

            // ---------------------------------------------------------------------
            // SHEET 3: THỐNG KÊ THEO LỚP HỌC
            // ---------------------------------------------------------------------
            var ws3 = workbook.Worksheets.Add("LopHoc_DangKy");
            ws3.ShowGridLines = true;

            ws3.Range("A1:M1").Merge().Value = "BÁO CÁO THỐNG KÊ TÌNH HÌNH TUYỂN SINH & HỌC PHÍ THEO LỚP HỌC";
            ws3.Cell("A1").Style.Font.Bold = true;
            ws3.Cell("A1").Style.Font.FontSize = 13;
            ws3.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws3.Cell("A1").Style.Fill.BackgroundColor = unetiPrimaryColor;
            ws3.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws3.Row(1).Height = 26;

            string[] headersLop = { "STT", "Mã lớp", "Tên lớp", "Khóa học", "Giảng viên phụ trách", "Lịch học", "Phòng học", "Số lượng tối đa", "Tổng đăng ký", "Đã xác nhận", "Tỷ lệ lấp đầy (%)", "Trạng thái lớp", "Doanh thu (VNĐ)" };
            for (int i = 0; i < headersLop.Length; i++)
            {
                var c = ws3.Cell(3, i + 1);
                c.Value = headersLop[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            int r3 = 3;
            int sttLop = 1;
            decimal tongDoanhThuLop = 0;
            int tongDangKyLop = 0;
            int tongDaXacNhanLop = 0;

            foreach (var lop in dsLopHoc.OrderBy(l => l.TenLop))
            {
                r3++;
                int tongDk = lop.DangKyHocs?.Count ?? 0;
                int xacNhan = lop.DangKyHocs?.Count(d => d.TrangThai == TrangThaiDangKy.DaXacNhan) ?? 0;
                tongDangKyLop += tongDk;
                tongDaXacNhanLop += xacNhan;

                double tyLeLapDay = lop.SoLuongToiDa > 0 ? (double)xacNhan / lop.SoLuongToiDa : 0;
                decimal doanhThuLop = lop.DangKyHocs?
                    .Where(d => d.TrangThai == TrangThaiDangKy.DaXacNhan)
                    .Sum(d => d.HocPhi) ?? 0;
                tongDoanhThuLop += doanhThuLop;

                ws3.Cell(r3, 1).Value = sttLop++;
                ws3.Cell(r3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws3.Cell(r3, 2).Value = lop.MaLop;
                ws3.Cell(r3, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws3.Cell(r3, 3).Value = lop.TenLop;
                ws3.Cell(r3, 4).Value = lop.KhoaHoc?.TenKhoaHoc ?? "";
                ws3.Cell(r3, 5).Value = lop.GiangVien?.HoTen ?? "Chưa phân công";
                ws3.Cell(r3, 6).Value = lop.LichHoc;
                ws3.Cell(r3, 7).Value = lop.PhongHoc;
                ws3.Cell(r3, 8).Value = lop.SoLuongToiDa;
                ws3.Cell(r3, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws3.Cell(r3, 9).Value = tongDk;
                ws3.Cell(r3, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws3.Cell(r3, 10).Value = xacNhan;
                ws3.Cell(r3, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws3.Cell(r3, 11).Value = tyLeLapDay;
                ws3.Cell(r3, 11).Style.NumberFormat.Format = "0.0%";
                ws3.Cell(r3, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws3.Cell(r3, 12).Value = lop.TrangThai;
                ws3.Cell(r3, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws3.Cell(r3, 13).Value = (double)doanhThuLop;
                ws3.Cell(r3, 13).Style.NumberFormat.Format = "#,##0 \"đ\"";
                ws3.Cell(r3, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                for (int c = 1; c <= 13; c++)
                {
                    ws3.Cell(r3, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws3.Cell(r3, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            // Dòng tổng cộng lớp
            r3++;
            ws3.Range(r3, 1, r3, 8).Merge().Value = "TỔNG CỘNG";
            ws3.Cell(r3, 1).Style.Font.Bold = true;
            ws3.Cell(r3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws3.Cell(r3, 9).Value = tongDangKyLop;
            ws3.Cell(r3, 9).Style.Font.Bold = true;
            ws3.Cell(r3, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws3.Cell(r3, 10).Value = tongDaXacNhanLop;
            ws3.Cell(r3, 10).Style.Font.Bold = true;
            ws3.Cell(r3, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws3.Cell(r3, 11).Value = "";
            ws3.Cell(r3, 12).Value = "";
            ws3.Cell(r3, 13).Value = (double)tongDoanhThuLop;
            ws3.Cell(r3, 13).Style.Font.Bold = true;
            ws3.Cell(r3, 13).Style.Font.FontColor = XLColor.FromArgb(22, 101, 52);
            ws3.Cell(r3, 13).Style.NumberFormat.Format = "#,##0 \"đ\"";
            ws3.Cell(r3, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            for (int c = 1; c <= 13; c++)
            {
                ws3.Cell(r3, c).Style.Fill.BackgroundColor = XLColor.FromArgb(241, 245, 249);
                ws3.Cell(r3, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws3.Cell(r3, c).Style.Border.OutsideBorderColor = borderColor;
            }

            ws3.Columns().AdjustToContents();

            // ---------------------------------------------------------------------
            // SHEET 4: CƠ CẤU TRẠNG THÁI ĐĂNG KÝ
            // ---------------------------------------------------------------------
            var ws4 = workbook.Worksheets.Add("TrangThai_DangKy");
            ws4.ShowGridLines = true;

            ws4.Range("A1:D1").Merge().Value = "CƠ CẤU TRẠNG THÁI ĐĂNG KÝ HỌC";
            ws4.Cell("A1").Style.Font.Bold = true;
            ws4.Cell("A1").Style.Font.FontSize = 13;
            ws4.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws4.Cell("A1").Style.Fill.BackgroundColor = unetiPrimaryColor;
            ws4.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws4.Row(1).Height = 26;

            string[] headersTT = { "STT", "Trạng thái đăng ký", "Số lượt đăng ký", "Tỷ lệ (%)" };
            for (int i = 0; i < headersTT.Length; i++)
            {
                var c = ws4.Cell(3, i + 1);
                c.Value = headersTT[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            var dsTT = new[]
            {
                ("1", TrangThaiDangKy.DaXacNhan, (double)dangKyDaXacNhan, pctXacNhan),
                ("2", TrangThaiDangKy.ChoXacNhan, (double)dangKyChoXacNhan, pctCho),
                ("3", TrangThaiDangKy.DaHuy, (double)dangKyDaHuy, pctHuy)
            };

            int r4 = 3;
            foreach (var item in dsTT)
            {
                r4++;
                ws4.Cell(r4, 1).Value = item.Item1;
                ws4.Cell(r4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws4.Cell(r4, 2).Value = item.Item2;
                ws4.Cell(r4, 3).Value = item.Item3;
                ws4.Cell(r4, 3).Style.NumberFormat.Format = "#,##0";
                ws4.Cell(r4, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws4.Cell(r4, 4).Value = item.Item4;
                ws4.Cell(r4, 4).Style.NumberFormat.Format = "0.0%";
                ws4.Cell(r4, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                for (int c = 1; c <= 4; c++)
                {
                    ws4.Cell(r4, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws4.Cell(r4, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            // Dòng tổng cộng trạng thái
            r4++;
            ws4.Cell(r4, 1).Value = "";
            ws4.Cell(r4, 2).Value = "TỔNG CỘNG";
            ws4.Cell(r4, 2).Style.Font.Bold = true;
            ws4.Cell(r4, 3).Value = (double)tongLuotDangKy;
            ws4.Cell(r4, 3).Style.Font.Bold = true;
            ws4.Cell(r4, 3).Style.NumberFormat.Format = "#,##0";
            ws4.Cell(r4, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws4.Cell(r4, 4).Value = 1.0;
            ws4.Cell(r4, 4).Style.Font.Bold = true;
            ws4.Cell(r4, 4).Style.NumberFormat.Format = "0.0%";
            ws4.Cell(r4, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            for (int c = 1; c <= 4; c++)
            {
                ws4.Cell(r4, c).Style.Fill.BackgroundColor = XLColor.FromArgb(241, 245, 249);
                ws4.Cell(r4, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws4.Cell(r4, c).Style.Border.OutsideBorderColor = borderColor;
            }

            ws4.Columns().AdjustToContents();

            // ---------------------------------------------------------------------
            // SHEET 5: DỮ LIỆU ĐĂNG KÝ HỌC CHI TIẾT
            // ---------------------------------------------------------------------
            var ws5 = workbook.Worksheets.Add("ChiTiet_DangKy");
            ws5.ShowGridLines = true;

            ws5.Range("A1:L1").Merge().Value = "DANH SÁCH CHI TIẾT TẤT CẢ LƯỢT ĐĂNG KÝ HỌC (ĐỐI SOÁT THỐNG KÊ)";
            ws5.Cell("A1").Style.Font.Bold = true;
            ws5.Cell("A1").Style.Font.FontSize = 13;
            ws5.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws5.Cell("A1").Style.Fill.BackgroundColor = unetiPrimaryColor;
            ws5.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws5.Row(1).Height = 26;

            string[] headersCT = { "STT", "Mã ĐK", "Mã HV", "Họ và tên học viên", "Số điện thoại", "Email", "Mã lớp", "Tên lớp học", "Khóa học", "Ngày đăng ký", "Học phí (VNĐ)", "Trạng thái" };
            for (int i = 0; i < headersCT.Length; i++)
            {
                var c = ws5.Cell(3, i + 1);
                c.Value = headersCT[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            int r5 = 3;
            int sttDK = 1;
            foreach (var dk in dsDangKy)
            {
                r5++;
                ws5.Cell(r5, 1).Value = sttDK++;
                ws5.Cell(r5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws5.Cell(r5, 2).Value = dk.MaDangKy;
                ws5.Cell(r5, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws5.Cell(r5, 3).Value = dk.MaHocVien;
                ws5.Cell(r5, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws5.Cell(r5, 4).Value = dk.HocVien?.HoTen ?? "";
                ws5.Cell(r5, 5).Value = dk.HocVien?.SoDienThoai ?? "";
                ws5.Cell(r5, 6).Value = dk.HocVien?.Email ?? "";
                ws5.Cell(r5, 7).Value = dk.MaLop;
                ws5.Cell(r5, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws5.Cell(r5, 8).Value = dk.LopHoc?.TenLop ?? "";
                ws5.Cell(r5, 9).Value = dk.LopHoc?.KhoaHoc?.TenKhoaHoc ?? "";
                ws5.Cell(r5, 10).Value = dk.NgayDangKy.ToString("dd/MM/yyyy");
                ws5.Cell(r5, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws5.Cell(r5, 11).Value = (double)dk.HocPhi;
                ws5.Cell(r5, 11).Style.NumberFormat.Format = "#,##0 \"đ\"";
                ws5.Cell(r5, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws5.Cell(r5, 12).Value = dk.TrangThai;
                ws5.Cell(r5, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int c = 1; c <= 12; c++)
                {
                    ws5.Cell(r5, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws5.Cell(r5, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            ws5.Columns().AdjustToContents();

            // Lưu và trả về file Excel
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"BaoCaoThongKe_UNETI_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // =========================================================================
        // CHỨC NĂNG 2: TẢI BIỂU MẪU EXCEL CHUẨN ĐỂ NHẬP DỮ LIỆU
        // =========================================================================
        public async Task<IActionResult> DownloadTemplate()
        {
            var dsLop = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .OrderBy(l => l.TenLop)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var unetiPrimaryColor = XLColor.FromArgb(30, 58, 138);
            var borderColor = XLColor.FromArgb(203, 213, 225);

            // Sheet 1: Mẫu nhập dữ liệu
            var ws1 = workbook.Worksheets.Add("NhapDangKyHoc");
            ws1.ShowGridLines = true;

            // Hướng dẫn
            ws1.Range("A1:J1").Merge().Value = "BIỂU MẪU NHẬP DỮ LIỆU ĐĂNG KÝ HỌC VIÊN & CẬP NHẬT BÁO CÁO THỐNG KÊ (Dấu * là bắt buộc)";
            ws1.Cell("A1").Style.Font.Bold = true;
            ws1.Cell("A1").Style.Font.FontSize = 12;
            ws1.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws1.Cell("A1").Style.Fill.BackgroundColor = unetiPrimaryColor;
            ws1.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws1.Row(1).Height = 25;

            string[] headers =
            {
                "Họ và tên (*)",
                "Số điện thoại (*)",
                "Email (*)",
                "Ngày sinh (dd/MM/yyyy)",
                "Giới tính (Nam/Nữ)",
                "Địa chỉ",
                "Tên lớp hoặc Mã lớp (*)",
                "Ngày đăng ký (dd/MM/yyyy)",
                "Học phí (VNĐ)",
                "Trạng thái (Đã xác nhận/Chờ xác nhận)"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var c = ws1.Cell(2, i + 1);
                c.Value = headers[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            // Dòng dữ liệu mẫu để người dùng dễ hiểu
            var firstClass = dsLop.FirstOrDefault();
            var sampleClass = firstClass?.TenLop ?? "Tiếng Anh Giao Tiếp A1 - K01";
            var sampleFee = (double)(firstClass?.HocPhi ?? 2500000m);
            var sampleDate = DateTime.Now.ToString("dd/MM/yyyy");

            var sampleRows = new[]
            {
                new object[] { "Nguyễn Văn An", "0912345678", "nguyenvanan@gmail.com", "15/05/2002", "Nam", "Hà Nội", sampleClass, sampleDate, sampleFee, "Đã xác nhận" },
                new object[] { "Trần Thị Mai", "0987654321", "tranmai@gmail.com", "20/08/2003", "Nữ", "Hà Nam", sampleClass, sampleDate, sampleFee, "Đã xác nhận" },
                new object[] { "Lê Hoàng Long", "0905123456", "hoanglong@gmail.com", "10/12/2001", "Nam", "Nam Định", firstClass != null ? firstClass.MaLop.ToString() : "1", sampleDate, sampleFee, "Chờ xác nhận" }
            };

            int r = 2;
            foreach (var row in sampleRows)
            {
                r++;
                for (int c = 0; c < row.Length; c++)
                {
                    var cell = ws1.Cell(r, c + 1);
                    if (row[c] is double d)
                    {
                        cell.Value = d;
                        cell.Style.NumberFormat.Format = "#,##0";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    else
                    {
                        cell.Value = row[c].ToString();
                    }

                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = borderColor;
                }
            }

            ws1.Columns().AdjustToContents();

            // Sheet 2: Danh sách lớp học hiện có để tham khảo
            var ws2 = workbook.Worksheets.Add("DanhSachLopHocHienCo");
            ws2.ShowGridLines = true;

            ws2.Range("A1:G1").Merge().Value = "DANH SÁCH LỚP HỌC HIỆN CÓ ĐỂ ĐIỀN VÀO CỘT 'TÊN LỚP HOẶC MÃ LỚP'";
            ws2.Cell("A1").Style.Font.Bold = true;
            ws2.Cell("A1").Style.Font.FontSize = 12;
            ws2.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws2.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromArgb(2, 132, 199);
            ws2.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws2.Row(1).Height = 25;

            string[] headersLop = { "Mã lớp", "Tên lớp học", "Khóa học", "Học phí quy định (VNĐ)", "Lịch học", "Phòng học", "Trạng thái tuyển sinh" };
            for (int i = 0; i < headersLop.Length; i++)
            {
                var c = ws2.Cell(2, i + 1);
                c.Value = headersLop[i];
                c.Style.Font.Bold = true;
                c.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                c.Style.Border.OutsideBorderColor = borderColor;
            }

            int r2 = 2;
            foreach (var lop in dsLop)
            {
                r2++;
                ws2.Cell(r2, 1).Value = lop.MaLop;
                ws2.Cell(r2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws2.Cell(r2, 2).Value = lop.TenLop;
                ws2.Cell(r2, 3).Value = lop.KhoaHoc?.TenKhoaHoc ?? "";
                ws2.Cell(r2, 4).Value = (double)lop.HocPhi;
                ws2.Cell(r2, 4).Style.NumberFormat.Format = "#,##0 \"đ\"";
                ws2.Cell(r2, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws2.Cell(r2, 5).Value = lop.LichHoc;
                ws2.Cell(r2, 6).Value = lop.PhongHoc;
                ws2.Cell(r2, 7).Value = lop.TrangThai;
                ws2.Cell(r2, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int c = 1; c <= 7; c++)
                {
                    ws2.Cell(r2, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws2.Cell(r2, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            ws2.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Nhap_DuLieu_ThongKe_UNETI.xlsx");
        }

        // =========================================================================
        // CHỨC NĂNG 3: NHẬP DỮ LIỆU TỪ EXCEL VÀ CẬP NHẬT BÁO CÁO THỐNG KÊ
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile? fileExcel)
        {
            if (fileExcel == null || fileExcel.Length == 0)
            {
                TempData["Loi"] = "Vui lòng chọn một file Excel (.xlsx hoặc .xls) để nhập dữ liệu.";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(fileExcel.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                TempData["Loi"] = "Định dạng file không hợp lệ! Vui lòng chỉ tải lên file Excel có phần mở rộng .xlsx hoặc .xls.";
                return RedirectToAction(nameof(Index));
            }

            var result = new ImportThongKeResult();

            try
            {
                using var stream = fileExcel.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                // Ưu tiên sheet có tên NhapDangKyHoc hoặc sheet đầu tiên
                var ws = workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("Nhap") || w.Name.Contains("DangKy"))
                         ?? workbook.Worksheets.FirstOrDefault();

                if (ws == null)
                {
                    TempData["Loi"] = "File Excel không chứa bất kỳ trang tính (Worksheet) nào.";
                    return RedirectToAction(nameof(Index));
                }

                // Xác định dòng tiêu đề (quét 5 dòng đầu)
                int headerRow = 1;
                for (int r = 1; r <= Math.Min(5, ws.LastRowUsed()?.RowNumber() ?? 1); r++)
                {
                    var textRow = ws.Row(r).Cells().Select(c => c.GetString().ToLower()).ToList();
                    if (textRow.Any(t => t.Contains("họ") || t.Contains("hoten") || t.Contains("tên") || t.Contains("lớp") || t.Contains("lop")))
                    {
                        headerRow = r;
                        break;
                    }
                }

                int lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
                int startRow = headerRow + 1;

                if (startRow > lastRow)
                {
                    TempData["Loi"] = "File Excel không chứa dòng dữ liệu nào sau tiêu đề.";
                    return RedirectToAction(nameof(Index));
                }

                // Cache danh sách lớp học và tài khoản
                var dsLop = await _context.LopHocs
                    .Include(l => l.KhoaHoc)
                    .ToListAsync();

                for (int row = startRow; row <= lastRow; row++)
                {
                    var r = ws.Row(row);
                    // Bỏ qua dòng hoàn toàn trống
                    if (r.IsEmpty() || r.Cells().All(c => string.IsNullOrWhiteSpace(c.GetString())))
                    {
                        continue;
                    }

                    result.TongSoDong++;

                    // Đọc từng cột theo thứ tự chuẩn
                    string hoTen = ws.Cell(row, 1).GetString().Trim();
                    string soDienThoai = ws.Cell(row, 2).GetString().Trim();
                    string email = ws.Cell(row, 3).GetString().Trim();
                    string strNgaySinh = ws.Cell(row, 4).GetString().Trim();
                    string gioiTinh = ws.Cell(row, 5).GetString().Trim();
                    string diaChi = ws.Cell(row, 6).GetString().Trim();
                    string lopInput = ws.Cell(row, 7).GetString().Trim();
                    string strNgayDangKy = ws.Cell(row, 8).GetString().Trim();
                    string strHocPhi = ws.Cell(row, 9).GetString().Trim();
                    string trangThaiInput = ws.Cell(row, 10).GetString().Trim();

                    // Kiểm tra Họ tên
                    if (string.IsNullOrWhiteSpace(hoTen))
                    {
                        result.SoLoi++;
                        result.DanhSachLoi.Add($"Dòng {row}: Họ và tên học viên không được để trống.");
                        continue;
                    }

                    // Kiểm tra Lớp học
                    if (string.IsNullOrWhiteSpace(lopInput))
                    {
                        result.SoLoi++;
                        result.DanhSachLoi.Add($"Dòng {row}: Cột lớp học không được để trống.");
                        continue;
                    }

                    // Tìm lớp học theo Mã lớp hoặc Tên lớp
                    LopHoc? lop = null;
                    if (int.TryParse(lopInput, out int maLopTim))
                    {
                        lop = dsLop.FirstOrDefault(l => l.MaLop == maLopTim);
                    }
                    if (lop == null)
                    {
                        lop = dsLop.FirstOrDefault(l => l.TenLop.Trim().Equals(lopInput, StringComparison.OrdinalIgnoreCase))
                           ?? dsLop.FirstOrDefault(l => l.TenLop.Contains(lopInput, StringComparison.OrdinalIgnoreCase));
                    }

                    if (lop == null)
                    {
                        result.SoLoi++;
                        result.DanhSachLoi.Add($"Dòng {row}: Không tìm thấy lớp học '{lopInput}' trong cơ sở dữ liệu.");
                        continue;
                    }

                    // Xử lý Số điện thoại và Email mặc định nếu thiếu
                    if (string.IsNullOrWhiteSpace(soDienThoai))
                    {
                        soDienThoai = "09" + Random.Shared.Next(10000000, 99999999);
                    }
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        var cleanName = Regex.Replace(hoTen.ToLower(), @"\s+", "");
                        email = $"{cleanName}{Random.Shared.Next(100, 999)}@uneti.edu.vn";
                    }

                    // Parse Ngày sinh
                    DateTime ngaySinh = DateTime.Today.AddYears(-20);
                    if (!string.IsNullOrWhiteSpace(strNgaySinh))
                    {
                        if (ws.Cell(row, 4).TryGetValue(out DateTime dtSinh))
                        {
                            ngaySinh = dtSinh;
                        }
                        else if (DateTime.TryParse(strNgaySinh, out DateTime parsedSinh))
                        {
                            ngaySinh = parsedSinh;
                        }
                    }

                    // Giới tính
                    if (string.IsNullOrWhiteSpace(gioiTinh))
                    {
                        gioiTinh = "Nam";
                    }

                    // Parse Ngày đăng ký
                    DateTime ngayDangKy = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(strNgayDangKy))
                    {
                        if (ws.Cell(row, 8).TryGetValue(out DateTime dtDk))
                        {
                            ngayDangKy = dtDk;
                        }
                        else if (DateTime.TryParse(strNgayDangKy, out DateTime parsedDk))
                        {
                            ngayDangKy = parsedDk;
                        }
                    }

                    // Parse Học phí
                    decimal hocPhi = lop.HocPhi;
                    if (!string.IsNullOrWhiteSpace(strHocPhi))
                    {
                        if (ws.Cell(row, 9).TryGetValue(out decimal decHp) && decHp > 0)
                        {
                            hocPhi = decHp;
                        }
                        else
                        {
                            string cleanHp = Regex.Replace(strHocPhi, @"[^\d\.]", "");
                            if (decimal.TryParse(cleanHp, out decimal parsedHp) && parsedHp > 0)
                            {
                                hocPhi = parsedHp;
                            }
                        }
                    }

                    // Parse Trạng thái
                    string trangThai = TrangThaiDangKy.DaXacNhan; // Mặc định cho dữ liệu nhập báo cáo
                    if (!string.IsNullOrWhiteSpace(trangThaiInput))
                    {
                        string lowerTt = trangThaiInput.ToLower();
                        if (lowerTt.Contains("chờ") || lowerTt.Contains("cho"))
                            trangThai = TrangThaiDangKy.ChoXacNhan;
                        else if (lowerTt.Contains("hủy") || lowerTt.Contains("huy"))
                            trangThai = TrangThaiDangKy.DaHuy;
                        else
                            trangThai = TrangThaiDangKy.DaXacNhan;
                    }

                    // Tìm hoặc tạo mới Học viên
                    var hocVien = await _context.HocViens
                        .Include(h => h.TaiKhoan)
                        .FirstOrDefaultAsync(h => h.Email == email || h.SoDienThoai == soDienThoai);

                    if (hocVien == null)
                    {
                        // Tạo tài khoản cho học viên
                        string baseUser = email.Contains('@') ? email.Split('@')[0] : ("hv" + soDienThoai);
                        baseUser = Regex.Replace(baseUser.ToLower(), @"[^a-z0-9]", "");
                        if (baseUser.Length < 3) baseUser += "user";

                        string tenDangNhap = baseUser;
                        int suffix = 1;
                        while (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap))
                        {
                            tenDangNhap = $"{baseUser}{suffix++}";
                        }

                        var tk = new TaiKhoan
                        {
                            TenDangNhap = tenDangNhap,
                            MatKhau = "123456",
                            HoTen = hoTen,
                            Email = email,
                            VaiTro = "HocVien",
                            TrangThai = true
                        };
                        _context.TaiKhoans.Add(tk);
                        await _context.SaveChangesAsync();

                        hocVien = new HocVien
                        {
                            MaTaiKhoan = tk.MaTaiKhoan,
                            HoTen = hoTen,
                            SoDienThoai = soDienThoai,
                            Email = email,
                            GioiTinh = gioiTinh,
                            NgaySinh = ngaySinh,
                            DiaChi = diaChi
                        };
                        _context.HocViens.Add(hocVien);
                        await _context.SaveChangesAsync();
                    }

                    // Kiểm tra đăng ký học lớp này
                    var dangKy = await _context.DangKyHocs
                        .FirstOrDefaultAsync(d => d.MaHocVien == hocVien.MaHocVien && d.MaLop == lop.MaLop);

                    if (dangKy != null)
                    {
                        // Đã có bản ghi -> cập nhật trạng thái và học phí
                        dangKy.TrangThai = trangThai;
                        dangKy.HocPhi = hocPhi;
                        dangKy.NgayDangKy = ngayDangKy;
                        result.SoThanhCong++;
                        result.ChiTietThanhCong.Add($"Dòng {row}: Cập nhật đăng ký lớp '{lop.TenLop}' cho học viên '{hoTen}'.");
                    }
                    else
                    {
                        // Thêm mới đăng ký
                        var dangKyMoi = new DangKyHoc
                        {
                            MaHocVien = hocVien.MaHocVien,
                            MaLop = lop.MaLop,
                            NgayDangKy = ngayDangKy,
                            HocPhi = hocPhi,
                            TrangThai = trangThai
                        };
                        _context.DangKyHocs.Add(dangKyMoi);
                        result.SoThanhCong++;
                        result.ChiTietThanhCong.Add($"Dòng {row}: Thêm mới đăng ký lớp '{lop.TenLop}' cho học viên '{hoTen}'.");
                    }

                    await _context.SaveChangesAsync();
                }

                // Lưu kết quả vào TempData
                TempData["ImportResultJson"] = JsonSerializer.Serialize(result);

                if (result.SoThanhCong > 0)
                {
                    TempData["ThanhCong"] = $"Nhập Excel thành công: Đã ghi nhận / cập nhật {result.SoThanhCong} lượt đăng ký vào hệ thống!";
                }
                if (result.SoLoi > 0)
                {
                    TempData["Loi"] = $"Quá trình nhập có {result.SoLoi} dòng bị lỗi. Vui lòng kiểm tra chi tiết danh sách bên dưới.";
                }
            }
            catch (Exception ex)
            {
                TempData["Loi"] = $"Đã xảy ra lỗi khi đọc file Excel: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
