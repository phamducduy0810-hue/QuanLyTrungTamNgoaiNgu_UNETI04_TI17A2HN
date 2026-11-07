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
        // [MỚI] 1. XUẤT BÁO CÁO THỐNG KÊ RA FILE EXCEL (4 sheet) - dùng ClosedXML
        // =========================================================================
        // GET: /ThongKe/ExportExcel
        public async Task<IActionResult> ExportExcel()
        {
            // Bước 1: lấy dữ liệu từ database
            var dsKhoaHoc = await _context.KhoaHocs.Include(k => k.LopHocs).ToListAsync();
            var dsLop = await _context.LopHocs.Include(l => l.DangKyHocs).ToListAsync();
            var dsDangKy = await _context.DangKyHocs.Include(d => d.LopHoc).ToListAsync();
            int tongHocVien = await _context.HocViens.CountAsync();

            // Học phí chỉ tính các đăng ký "Đã xác nhận"
            var daXacNhan = dsDangKy.Where(d => d.TrangThai == TrangThaiDangKy.DaXacNhan).ToList();

            // Bước 2: tạo file Excel gồm 4 sheet
            using var wb = new XLWorkbook();

            // Sheet 1: Tổng quan
            TaoSheet(wb, "TongQuan", "TỔNG QUAN HOẠT ĐỘNG TRUNG TÂM", new[] { "Chỉ số", "Giá trị" }, new List<object[]>
            {
                new object[] { "Tổng số khóa học", dsKhoaHoc.Count },
                new object[] { "Tổng số lớp học", dsLop.Count },
                new object[] { "Tổng số học viên", tongHocVien },
                new object[] { "Tổng lượt đăng ký", dsDangKy.Count },
                new object[] { "Tổng học phí đã xác nhận (đ)", (double)daXacNhan.Sum(d => d.HocPhi) }
            });

            // Sheet 2: Số lớp và học phí theo từng khóa học
            var dongKhoaHoc = dsKhoaHoc.Select(k => new object[]
            {
                k.TenKhoaHoc,
                k.LopHocs?.Count ?? 0,
                (double)daXacNhan.Where(d => d.LopHoc!.MaKhoaHoc == k.MaKhoaHoc).Sum(d => d.HocPhi)
            }).ToList();
            TaoSheet(wb, "TheoKhoaHoc", "SỐ LỚP VÀ HỌC PHÍ THEO KHÓA HỌC",
                new[] { "Khóa học", "Số lớp", "Học phí (đ)" }, dongKhoaHoc, coTong: true);

            // Sheet 3: Đăng ký và học phí theo từng lớp
            var dongLop = dsLop.Select(l => new object[]
            {
                l.TenLop,
                l.DangKyHocs?.Count ?? 0,
                l.DangKyHocs?.Count(d => d.TrangThai == TrangThaiDangKy.DaXacNhan) ?? 0,
                (double)daXacNhan.Where(d => d.MaLop == l.MaLop).Sum(d => d.HocPhi)
            }).ToList();
            TaoSheet(wb, "TheoLop", "ĐĂNG KÝ VÀ HỌC PHÍ THEO LỚP HỌC",
                new[] { "Lớp học", "Tổng đăng ký", "Đã xác nhận", "Học phí (đ)" }, dongLop, coTong: true);

            // Sheet 4: Số đăng ký theo từng trạng thái
            var dongTrangThai = TrangThaiDangKy.TatCa.Select(tt => new object[]
            {
                tt,
                dsDangKy.Count(d => d.TrangThai == tt)
            }).ToList();
            TaoSheet(wb, "TheoTrangThai", "SỐ ĐĂNG KÝ THEO TRẠNG THÁI",
                new[] { "Trạng thái", "Số đăng ký" }, dongTrangThai, coTong: true);

            // Bước 3: trả file về cho người dùng tải
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // Hàm phụ: tạo 1 sheet đẹp gồm banner tiêu đề, dòng thời gian, bảng có viền, dòng tổng cộng
        // - tieuDeSheet = null: không có banner, tiêu đề cột nằm ở dòng 1 (dùng cho file mẫu để nhập)
        // - coTong = true: thêm dòng "TỔNG CỘNG" cộng các cột số
        private static void TaoSheet(XLWorkbook wb, string tenSheet, string? tieuDeSheet,
            string[] tieuDe, List<object[]> dong, bool coTong = false)
        {
            var ws = wb.Worksheets.Add(tenSheet);
            int soCot = tieuDe.Length;
            int dongTieuDe = 1; // dòng chứa tiêu đề cột
            var xanhDam = XLColor.FromArgb(30, 58, 138);
            var xanhNhat = XLColor.FromArgb(2, 132, 199);
            var xam = XLColor.FromArgb(241, 245, 249);

            // 1. Banner tiêu đề báo cáo (dòng 1) và thời gian xuất (dòng 2)
            if (tieuDeSheet != null)
            {
                ws.Range(1, 1, 1, soCot).Merge().Value = tieuDeSheet;
                var banner = ws.Range(1, 1, 1, soCot);
                banner.Style.Font.Bold = true;
                banner.Style.Font.FontSize = 14;
                banner.Style.Font.FontColor = XLColor.White;
                banner.Style.Fill.BackgroundColor = xanhDam;
                banner.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(1).Height = 28;

                ws.Range(2, 1, 2, soCot).Merge().Value = $"Xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range(2, 1, 2, soCot).Style.Font.Italic = true;
                ws.Range(2, 1, 2, soCot).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                dongTieuDe = 4; // chừa dòng 3 trống
            }

            // 2. Tiêu đề cột: chữ trắng, nền xanh, căn giữa
            for (int i = 0; i < soCot; i++)
                ws.Cell(dongTieuDe, i + 1).Value = tieuDe[i];
            var hang = ws.Range(dongTieuDe, 1, dongTieuDe, soCot);
            hang.Style.Font.Bold = true;
            hang.Style.Font.FontColor = XLColor.White;
            hang.Style.Fill.BackgroundColor = xanhNhat;
            hang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 3. Dữ liệu: số hiển thị dạng 1,000,000, dòng chẵn tô màu xám nhạt cho dễ nhìn
            int cuoi = dongTieuDe + dong.Count; // dòng dữ liệu cuối cùng
            if (dong.Count > 0)
            {
                ws.Cell(dongTieuDe + 1, 1).InsertData(dong);
                ws.Range(dongTieuDe + 1, 1, cuoi, soCot).Style.NumberFormat.Format = "#,##0";
                for (int r = dongTieuDe + 2; r <= cuoi; r += 2)
                    ws.Range(r, 1, r, soCot).Style.Fill.BackgroundColor = xam;
            }

            // 4. Dòng tổng cộng: cộng các cột có dữ liệu là số
            if (coTong && dong.Count > 0)
            {
                cuoi++;
                ws.Cell(cuoi, 1).Value = "TỔNG CỘNG";
                for (int c = 1; c < soCot; c++)
                    if (dong[0][c] is not string)
                        ws.Cell(cuoi, c + 1).Value = dong.Sum(d => Convert.ToDouble(d[c]));
                var tong = ws.Range(cuoi, 1, cuoi, soCot);
                tong.Style.Font.Bold = true;
                tong.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
            }

            // 5. Kẻ viền toàn bộ bảng, cố định dòng tiêu đề khi cuộn
            var bang = ws.Range(dongTieuDe, 1, cuoi, soCot);
            bang.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            bang.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            bang.Style.Border.OutsideBorderColor = XLColor.FromArgb(203, 213, 225);
            bang.Style.Border.InsideBorderColor = XLColor.FromArgb(203, 213, 225);
            ws.SheetView.FreezeRows(dongTieuDe);

            // 6. Giãn cột vừa nội dung (chỉ tính phần bảng), cộng thêm lề cho thoáng
            for (int c = 1; c <= soCot; c++)
            {
                ws.Column(c).AdjustToContents(dongTieuDe, cuoi);
                ws.Column(c).Width = Math.Max(ws.Column(c).Width + 3, 20);
            }
        }

        // =========================================================================
        // [MỚI] 2a. TẢI FILE EXCEL MẪU ĐỂ NHẬP ĐĂNG KÝ HỌC
        // =========================================================================
        // GET: /ThongKe/DownloadTemplate
        public IActionResult DownloadTemplate()
        {
            using var wb = new XLWorkbook();

            // Sheet nhập liệu: dòng 1 là tiêu đề, người dùng điền dữ liệu từ dòng 2
            TaoSheet(wb, "DangKyHoc", null,
                new[] { "MaHocVien", "MaLop", "NgayDangKy", "HocPhi", "TrangThai" },
                new List<object[]>());

            // Sheet hướng dẫn cách điền
            var hd = wb.Worksheets.Add("HuongDan");
            hd.Cell(1, 1).Value = "Mỗi dòng ở sheet DangKyHoc là một lượt đăng ký, điền từ dòng 2.";
            hd.Cell(2, 1).Value = "MaHocVien, MaLop: mã đã có trong hệ thống (bắt buộc). Học viên chưa có sẽ bị bỏ qua.";
            hd.Cell(3, 1).Value = "NgayDangKy: dạng dd/MM/yyyy. Để trống = hôm nay.";
            hd.Cell(4, 1).Value = "HocPhi: để trống = học phí của lớp.";
            hd.Cell(5, 1).Value = "TrangThai: Chờ xác nhận / Đã xác nhận / Đã hủy. Để trống = Chờ xác nhận.";
            hd.Column(1).AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MauNhapDangKy.xlsx");
        }

        // =========================================================================
        // [MỚI] 2b. NHẬP HÀNG LOẠT TỪ FILE EXCEL VÀO SQL SERVER
        // =========================================================================
        // POST: /ThongKe/ImportExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile? fileExcel)
        {
            // Bước 1: kiểm tra đã chọn file .xlsx chưa
            if (fileExcel == null || fileExcel.Length == 0
                || Path.GetExtension(fileExcel.FileName).ToLower() != ".xlsx")
            {
                TempData["Loi"] = "Vui lòng chọn file Excel (.xlsx).";
                return RedirectToAction(nameof(Index));
            }

            var kq = new ImportThongKeResult(); // lưu kết quả nhập để hiển thị lại

            try
            {
                // Bước 2: mở file, đọc sheet đầu tiên
                using var wb = new XLWorkbook(fileExcel.OpenReadStream());
                var ws = wb.Worksheets.First();
                int dongCuoi = ws.LastRowUsed()?.RowNumber() ?? 1;

                // Bước 3: đọc từng dòng dữ liệu (bỏ dòng 1 là tiêu đề)
                for (int dong = 2; dong <= dongCuoi; dong++)
                {
                    if (ws.Row(dong).IsEmpty()) continue; // bỏ qua dòng trống

                    kq.TongSoDong++;
                    string? loi = await ThemMotDangKy(ws, dong);

                    if (loi == null)
                    {
                        kq.SoThanhCong++;
                    }
                    else
                    {
                        kq.SoLoi++;
                        kq.DanhSachLoi.Add($"Dòng {dong}: {loi}");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Loi"] = "Không đọc được file Excel: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

            // Bước 4: báo kết quả (chỉ giữ 10 lỗi đầu để TempData không quá nặng)
            kq.DanhSachLoi = kq.DanhSachLoi.Take(10).ToList();
            TempData["ImportResultJson"] = JsonSerializer.Serialize(kq);

            if (kq.SoThanhCong > 0)
                TempData["ThanhCong"] = $"Đã nhập thành công {kq.SoThanhCong} đăng ký.";
            if (kq.SoLoi > 0)
                TempData["Loi"] = $"Có {kq.SoLoi} dòng bị bỏ qua, xem chi tiết bên dưới.";

            return RedirectToAction(nameof(Index));
        }

        // Hàm phụ: đọc 1 dòng Excel, kiểm tra rồi thêm 1 đăng ký.
        // Trả về null nếu thành công, hoặc nội dung lỗi nếu dòng đó không hợp lệ.
        private async Task<string?> ThemMotDangKy(IXLWorksheet ws, int dong)
        {
            // Cột 1, 2: mã học viên và mã lớp phải là số
            if (!int.TryParse(ws.Cell(dong, 1).GetString().Trim(), out int maHocVien)
                || !int.TryParse(ws.Cell(dong, 2).GetString().Trim(), out int maLop))
                return "MaHocVien và MaLop phải là số.";

            // Học viên chưa có trong hệ thống thì bỏ qua dòng này
            if (!await _context.HocViens.AnyAsync(h => h.MaHocVien == maHocVien))
                return $"Không có học viên mã {maHocVien}.";

            var lop = await _context.LopHocs.FindAsync(maLop);
            if (lop == null)
                return $"Không có lớp mã {maLop}.";

            // Không cho đăng ký trùng (cùng học viên, cùng lớp)
            if (await _context.DangKyHocs.AnyAsync(d => d.MaHocVien == maHocVien && d.MaLop == maLop))
                return "Học viên đã đăng ký lớp này rồi.";

            // Cột 3: ngày đăng ký (để trống thì lấy hôm nay)
            DateTime ngayDangKy = DateTime.Today;
            var oNgay = ws.Cell(dong, 3);
            if (!oNgay.IsEmpty() && !oNgay.TryGetValue(out ngayDangKy)
                && !DateTime.TryParse(oNgay.GetString(), out ngayDangKy))
                return "Ngày đăng ký không hợp lệ.";

            // Cột 4: học phí (để trống thì lấy học phí của lớp)
            decimal hocPhi = lop.HocPhi;
            var oHocPhi = ws.Cell(dong, 4);
            if (!oHocPhi.IsEmpty() && !oHocPhi.TryGetValue(out hocPhi))
                return "Học phí không hợp lệ.";

            // Cột 5: trạng thái (để trống thì là "Chờ xác nhận")
            string trangThai = ws.Cell(dong, 5).GetString().Trim();
            if (trangThai == "") trangThai = TrangThaiDangKy.ChoXacNhan;
            if (!TrangThaiDangKy.TatCa.Contains(trangThai))
                return $"Trạng thái \"{trangThai}\" không hợp lệ.";

            // Dữ liệu hợp lệ -> thêm vào database
            _context.DangKyHocs.Add(new DangKyHoc
            {
                MaHocVien = maHocVien,
                MaLop = maLop,
                NgayDangKy = ngayDangKy,
                HocPhi = hocPhi,
                TrangThai = trangThai
            });
            await _context.SaveChangesAsync();
            return null;
        }
    }
}