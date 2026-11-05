// Họ và tên: Phạm Đức Duy
// Mã sinh viên: 23103100084
// Nội dung thực hiện: Module 2 - Quản lý lớp học, tìm kiếm, lọc, sắp xếp và phân trang
// Bổ sung: Kiểm tra trùng phòng học & ca học, trùng lịch giảng viên (Schedule Conflict Checking)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Filters;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Helpers;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models.ViewModels;
using System.IO;
using ClosedXML.Excel;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class LopHocsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LopHocsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper phân tích và chuyển đổi chuỗi học phí linh hoạt (hỗ trợ số thuần, định dạng VN 1.000.000, 1,000,000, kèm đ/VNĐ)
        public static decimal? ParseFee(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            
            // Giữ lại chỉ các chữ số, dấu chấm và dấu phẩy, loại bỏ mọi ký hiệu tiền tệ hoặc khoảng trắng
            var sb = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (char.IsDigit(c) || c == '.' || c == ',')
                {
                    sb.Append(c);
                }
            }
            string s = sb.ToString();
            if (string.IsNullOrWhiteSpace(s)) return null;

            if (s.Contains('.') && s.Contains(','))
            {
                if (s.LastIndexOf(',') > s.LastIndexOf('.'))
                {
                    // Định dạng VN: 2.000.000,00 -> 2000000.00
                    s = s.Replace(".", "").Replace(',', '.');
                }
                else
                {
                    // Định dạng Quốc tế: 2,000,000.00 -> 2000000.00
                    s = s.Replace(",", "");
                }
            }
            else if (s.Contains('.'))
            {
                var parts = s.Split('.');
                if (parts.Length > 2)
                {
                    s = s.Replace(".", "");
                }
                else if (parts.Length == 2)
                {
                    if (parts[1].Length == 3 && parts[0].Length <= 3)
                    {
                        s = s.Replace(".", "");
                    }
                    else if (parts[1].Length <= 2)
                    {
                        // Giữ dấu chấm thập phân
                    }
                    else
                    {
                        s = s.Replace(".", "");
                    }
                }
            }
            else if (s.Contains(','))
            {
                var parts = s.Split(',');
                if (parts.Length > 2)
                {
                    s = s.Replace(",", "");
                }
                else if (parts.Length == 2)
                {
                    if (parts[1].Length <= 2)
                    {
                        s = parts[0] + "." + parts[1];
                    }
                    else
                    {
                        s = s.Replace(",", "");
                    }
                }
            }

            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return null;
        }

        // GET: LopHocs
        public async Task<IActionResult> Index(
            string? searchKeyword,
            int? maKhoaHoc,
            string? ngoaiNgu,
            string? trangThai,
            string? giaTu,
            string? giaDen,
            string? sortOrder,
            int currentPage = 1,
            int? page = null)
        {
            int pageSize = 5;
            int p = page ?? currentPage;
            if (p < 1) p = 1;

            // Xử lý chuyển đổi khoảng học phí (Từ - Đến) với parser hỗ trợ mọi định dạng
            decimal? minPrice = ParseFee(giaTu);
            decimal? maxPrice = ParseFee(giaDen);

            // Tự động hoán đổi nếu người dùng nhập ngược (VD: Từ 5 triệu - Đến 2 triệu)
            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            var query = _context.LopHocs.Include(l => l.KhoaHoc).Include(l => l.GiangVien).AsQueryable();

            // 1. Tìm kiếm theo Tên lớp hoặc Tên khóa học (LINQ)
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim().ToLower();
                query = query.Where(l => l.TenLop.ToLower().Contains(kw) || 
                                        (l.KhoaHoc != null && l.KhoaHoc.TenKhoaHoc.ToLower().Contains(kw)));
            }

            // 2. Lọc theo Khóa học
            if (maKhoaHoc.HasValue && maKhoaHoc.Value > 0)
            {
                query = query.Where(l => l.MaKhoaHoc == maKhoaHoc.Value);
            }

            // 3. Lọc theo Ngoại ngữ
            if (!string.IsNullOrWhiteSpace(ngoaiNgu))
            {
                string nn = ngoaiNgu.Trim();
                query = query.Where(l => l.KhoaHoc != null && (l.KhoaHoc.NgoaiNgu == nn || l.KhoaHoc.NgoaiNgu.Contains(nn)));
            }

            // 4. Lọc theo Trạng thái lớp
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                string tt = trangThai.Trim();
                query = query.Where(l => l.TrangThai == tt);
            }

            // 5. Lọc theo Khoảng học phí (Từ - Đến)
            if (minPrice.HasValue)
            {
                query = query.Where(l => l.HocPhi >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(l => l.HocPhi <= maxPrice.Value);
            }

            // 6. Sắp xếp (6 tiêu chí chuẩn đề bài: Tên A-Z, Tên Z-A, Học phí thấp-cao, Học phí cao-thấp, Khai giảng tăng, Khai giảng giảm)
            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(l => l.TenLop);
                    break;
                case "price_asc":
                    query = query.OrderBy(l => l.HocPhi);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(l => l.HocPhi);
                    break;
                case "date_asc":
                    query = query.OrderBy(l => l.NgayKhaiGiang);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(l => l.NgayKhaiGiang);
                    break;
                case "name_asc":
                default:
                    query = query.OrderBy(l => l.TenLop);
                    break;
            }

            // 7. Phân trang bằng Skip() và Take() trên LINQ EF Core
            int totalItems = await query.CountAsync();
            var danhSach = await query.Skip((p - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new LopHocIndexViewModel
            {
                DanhSachLop = danhSach,
                SearchKeyword = searchKeyword,
                MaKhoaHoc = maKhoaHoc,
                NgoaiNgu = ngoaiNgu,
                TrangThai = trangThai,
                GiaTu = minPrice,
                GiaDen = maxPrice,
                SortOrder = sortOrder ?? "name_asc",
                CurrentPage = p,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            // Dữ liệu dropdown lọc phục vụ View
            var dsKhoaHoc = await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync();
            var dsNgoaiNgu = await _context.KhoaHocs
                .Where(k => !string.IsNullOrEmpty(k.NgoaiNgu))
                .Select(k => k.NgoaiNgu)
                .Distinct()
                .ToListAsync();

            ViewBag.KhoaHocs = dsKhoaHoc;
            ViewBag.NgoaiNgus = dsNgoaiNgu;
            ViewBag.TrangThais = new List<string> { "Sắp mở", "Đang tuyển sinh", "Đang học", "Đã kết thúc" };

            ViewBag.KhoaHocList = new SelectList(dsKhoaHoc, "MaKhoaHoc", "TenKhoaHoc", maKhoaHoc);
            ViewBag.NgoaiNguList = new SelectList(dsNgoaiNgu, ngoaiNgu);
            ViewBag.TrangThaiList = new SelectList(new List<string> { "Sắp mở", "Đang tuyển sinh", "Đang học", "Đã kết thúc" }, trangThai);

            return View(viewModel);
        }
        // GET: LopHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lopHoc = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Include(l => l.DangKyHocs!)
                    .ThenInclude(d => d.HocVien)
                .FirstOrDefaultAsync(m => m.MaLop == id);
            if (lopHoc == null)
            {
                return NotFound();
            }

            return View(lopHoc);
        }

        // GET: LopHocs/XuatDanhSachHocVienExcel/5
        public async Task<IActionResult> XuatDanhSachHocVienExcel(int id)
        {
            var lopHoc = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Include(l => l.DangKyHocs!)
                    .ThenInclude(d => d.HocVien)
                .FirstOrDefaultAsync(l => l.MaLop == id);

            if (lopHoc == null)
            {
                return NotFound();
            }

            var dsDangKy = lopHoc.DangKyHocs?
                .OrderBy(d => d.HocVien?.HoTen ?? "")
                .ToList() ?? new List<DangKyHoc>();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DanhSachHocVien");
            ws.ShowGridLines = true;

            var unetiPrimaryColor = XLColor.FromArgb(30, 58, 138); // #1E3A8A
            var unetiSecondaryColor = XLColor.FromArgb(2, 132, 199); // #0284C7
            var borderColor = XLColor.FromArgb(203, 213, 225);

            // Banner tiêu đề
            ws.Range("A1:K1").Merge().Value = "TRUNG TÂM NGOẠI NGỮ UNETI - DỰ ÁN UNETI04 (TI17A2HN)";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 13;
            ws.Cell("A1").Style.Font.FontColor = XLColor.White;
            ws.Cell("A1").Style.Fill.BackgroundColor = unetiPrimaryColor;
            ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(1).Height = 26;

            ws.Range("A2:K2").Merge().Value = $"DANH SÁCH HỌC VIÊN & BẢNG ĐIỂM DANH LỚP: {lopHoc.TenLop.ToUpper()}";
            ws.Cell("A2").Style.Font.Bold = true;
            ws.Cell("A2").Style.Font.FontSize = 14;
            ws.Cell("A2").Style.Font.FontColor = XLColor.White;
            ws.Cell("A2").Style.Fill.BackgroundColor = unetiSecondaryColor;
            ws.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Height = 28;

            // Thông tin lớp học
            int rInfo = 4;
            ws.Cell(rInfo, 1).Value = "Khóa học:";
            ws.Cell(rInfo, 1).Style.Font.Bold = true;
            ws.Cell(rInfo, 2).Value = lopHoc.KhoaHoc?.TenKhoaHoc ?? "";
            ws.Cell(rInfo, 5).Value = "Giảng viên:";
            ws.Cell(rInfo, 5).Style.Font.Bold = true;
            ws.Cell(rInfo, 6).Value = lopHoc.GiangVien?.HoTen ?? "Chưa phân công";

            rInfo++;
            ws.Cell(rInfo, 1).Value = "Lịch học:";
            ws.Cell(rInfo, 1).Style.Font.Bold = true;
            ws.Cell(rInfo, 2).Value = lopHoc.LichHoc;
            ws.Cell(rInfo, 5).Value = "Phòng học:";
            ws.Cell(rInfo, 5).Style.Font.Bold = true;
            ws.Cell(rInfo, 6).Value = lopHoc.PhongHoc;

            rInfo++;
            ws.Cell(rInfo, 1).Value = "Khai giảng:";
            ws.Cell(rInfo, 1).Style.Font.Bold = true;
            ws.Cell(rInfo, 2).Value = lopHoc.NgayKhaiGiang.ToString("dd/MM/yyyy");
            ws.Cell(rInfo, 5).Value = "Sĩ số hiện tại:";
            ws.Cell(rInfo, 5).Style.Font.Bold = true;
            int daXacNhan = dsDangKy.Count(d => d.TrangThai == "Đã xác nhận");
            ws.Cell(rInfo, 6).Value = $"{daXacNhan} / {lopHoc.SoLuongToiDa} học viên (Tổng đăng ký: {dsDangKy.Count})";

            // Bảng danh sách học viên
            int rTable = 8;
            string[] headers =
            {
                "STT",
                "Mã HV",
                "Họ và tên",
                "Ngày sinh",
                "Giới tính",
                "Số điện thoại",
                "Email",
                "Ngày đăng ký",
                "Học phí",
                "Trạng thái học phí",
                "Ký tên / Điểm danh"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(rTable, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(224, 231, 255);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = borderColor;
            }
            ws.Row(rTable).Height = 24;

            int stt = 1;
            int rData = rTable;
            foreach (var dk in dsDangKy)
            {
                rData++;
                ws.Cell(rData, 1).Value = stt++;
                ws.Cell(rData, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(rData, 2).Value = dk.HocVien?.MaHocVien ?? dk.MaHocVien;
                ws.Cell(rData, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(rData, 3).Value = dk.HocVien?.HoTen ?? "";
                ws.Cell(rData, 3).Style.Font.Bold = true;

                ws.Cell(rData, 4).Value = dk.HocVien?.NgaySinh.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(rData, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(rData, 5).Value = dk.HocVien?.GioiTinh ?? "";
                ws.Cell(rData, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(rData, 6).Value = dk.HocVien?.SoDienThoai ?? "";
                ws.Cell(rData, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(rData, 7).Value = dk.HocVien?.Email ?? "";

                ws.Cell(rData, 8).Value = dk.NgayDangKy.ToString("dd/MM/yyyy");
                ws.Cell(rData, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(rData, 9).Value = (double)dk.HocPhi;
                ws.Cell(rData, 9).Style.NumberFormat.Format = "#,##0 \"đ\"";
                ws.Cell(rData, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(rData, 10).Value = dk.TrangThai;
                ws.Cell(rData, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                if (dk.TrangThai == "Đã xác nhận")
                {
                    ws.Cell(rData, 10).Style.Font.FontColor = XLColor.FromArgb(22, 101, 52);
                    ws.Cell(rData, 10).Style.Font.Bold = true;
                }

                ws.Cell(rData, 11).Value = ""; // Cột ký tên / điểm danh trống để in ấn

                for (int c = 1; c <= 11; c++)
                {
                    ws.Cell(rData, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(rData, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            if (!dsDangKy.Any())
            {
                rData++;
                ws.Range(rData, 1, rData, 11).Merge().Value = "Lớp học hiện tại chưa có học viên nào đăng ký.";
                ws.Cell(rData, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(rData, 1).Style.Font.Italic = true;
                for (int c = 1; c <= 11; c++)
                {
                    ws.Cell(rData, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(rData, c).Style.Border.OutsideBorderColor = borderColor;
                }
            }

            // Phần ký tên cuối trang
            int rSign = rData + 3;
            ws.Cell(rSign, 2).Value = "GIẢNG VIÊN PHỤ TRÁCH";
            ws.Cell(rSign, 2).Style.Font.Bold = true;
            ws.Cell(rSign, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(rSign, 9).Value = $"Hà Nội, ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
            ws.Cell(rSign, 9).Style.Font.Italic = true;
            ws.Cell(rSign, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(rSign + 1, 9).Value = "NGƯỜI LẬP DANH SÁCH";
            ws.Cell(rSign + 1, 9).Style.Font.Bold = true;
            ws.Cell(rSign + 1, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(rSign + 4, 2).Value = lopHoc.GiangVien?.HoTen ?? "(Ký và ghi rõ họ tên)";
            ws.Cell(rSign + 4, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var nguoiLap = HttpContext.Session.GetString("HoTen") ?? "Ban Quản lý Đào tạo";
            ws.Cell(rSign + 4, 9).Value = nguoiLap;
            ws.Cell(rSign + 4, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string cleanClassName = System.Text.RegularExpressions.Regex.Replace(lopHoc.TenLop, @"[^a-zA-Z0-9_\-]", "_");
            string fileName = $"DanhSachHocVien_{cleanClassName}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: LopHocs/Create (Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create()
        {
            ViewData["MaKhoaHoc"] = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc");
            ViewData["MaGiangVien"] = new SelectList(await _context.GiangViens.Where(g => g.TrangThai).OrderBy(g => g.HoTen).ToListAsync(), "MaGiangVien", "HoTen");
            var model = new LopHoc
            {
                NgayKhaiGiang = DateTime.Today.AddDays(7),
                TrangThai = "Đang tuyển sinh",
                SoLuongToiDa = 25,
                HocPhi = 1000000,
                PhongHoc = "Phòng A101",
                LichHoc = "T2 - T4 - T6 (18:00 - 20:00)"
            };
            return View(model);
        }

        // POST: LopHocs/Create (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create([Bind("MaLop,TenLop,MaKhoaHoc,MaGiangVien,NgayKhaiGiang,LichHoc,PhongHoc,SoLuongToiDa,HocPhi,TrangThai")] LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra trùng phòng học & ca học với các lớp đang hoạt động
                var trungPhong = await TimLopTrungPhongVaCaHocAsync(null, lopHoc.PhongHoc, lopHoc.LichHoc);
                if (trungPhong != null)
                {
                    ModelState.AddModelError("PhongHoc", $"Trùng phòng học! Phòng '{lopHoc.PhongHoc}' đã có lớp '{trungPhong.TenLop}' học vào ca này ({trungPhong.LichHoc}). Vui lòng đổi phòng hoặc chọn ca học khác.");
                }

                // 2. Kiểm tra trùng lịch giảng dạy của giảng viên (nếu có chọn giảng viên)
                if (lopHoc.MaGiangVien.HasValue && lopHoc.MaGiangVien.Value > 0)
                {
                    var trungGv = await TimLopTrungGiangVienVaCaHocAsync(null, lopHoc.MaGiangVien.Value, lopHoc.LichHoc);
                    if (trungGv != null)
                    {
                        ModelState.AddModelError("MaGiangVien", $"Trùng ca dạy! Giảng viên '{trungGv.GiangVien?.HoTen}' đã có lịch dạy lớp '{trungGv.TenLop}' ({trungGv.LichHoc}) tại phòng {trungGv.PhongHoc}.");
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Add(lopHoc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm mới lớp học '{lopHoc.TenLop}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["MaKhoaHoc"] = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc", lopHoc.MaKhoaHoc);
            ViewData["MaGiangVien"] = new SelectList(await _context.GiangViens.Where(g => g.TrangThai).OrderBy(g => g.HoTen).ToListAsync(), "MaGiangVien", "HoTen", lopHoc.MaGiangVien);
            return View(lopHoc);
        }

        // GET: LopHocs/Edit/5 (Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lopHoc = await _context.LopHocs.FindAsync(id);
            if (lopHoc == null)
            {
                return NotFound();
            }
            ViewData["MaKhoaHoc"] = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc", lopHoc.MaKhoaHoc);
            ViewData["MaGiangVien"] = new SelectList(await _context.GiangViens.Where(g => g.TrangThai).OrderBy(g => g.HoTen).ToListAsync(), "MaGiangVien", "HoTen", lopHoc.MaGiangVien);
            return View(lopHoc);
        }

        // POST: LopHocs/Edit/5 (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MaLop,TenLop,MaKhoaHoc,MaGiangVien,NgayKhaiGiang,LichHoc,PhongHoc,SoLuongToiDa,HocPhi,TrangThai")] LopHoc lopHoc)
        {
            if (id != lopHoc.MaLop)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 1. Kiểm tra trùng phòng học & ca học (ngoại trừ chính lớp này)
                var trungPhong = await TimLopTrungPhongVaCaHocAsync(lopHoc.MaLop, lopHoc.PhongHoc, lopHoc.LichHoc);
                if (trungPhong != null)
                {
                    ModelState.AddModelError("PhongHoc", $"Trùng phòng học! Phòng '{lopHoc.PhongHoc}' đã có lớp '{trungPhong.TenLop}' học vào ca này ({trungPhong.LichHoc}). Vui lòng đổi phòng hoặc chọn ca học khác.");
                }

                // 2. Kiểm tra trùng lịch giảng dạy của giảng viên (nếu có chọn giảng viên)
                if (lopHoc.MaGiangVien.HasValue && lopHoc.MaGiangVien.Value > 0)
                {
                    var trungGv = await TimLopTrungGiangVienVaCaHocAsync(lopHoc.MaLop, lopHoc.MaGiangVien.Value, lopHoc.LichHoc);
                    if (trungGv != null)
                    {
                        ModelState.AddModelError("MaGiangVien", $"Trùng ca dạy! Giảng viên '{trungGv.GiangVien?.HoTen}' đã có lịch dạy lớp '{trungGv.TenLop}' ({trungGv.LichHoc}) tại phòng {trungGv.PhongHoc}.");
                    }
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(lopHoc);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Cập nhật lớp học '{lopHoc.TenLop}' thành công!";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!LopHocExists(lopHoc.MaLop))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["MaKhoaHoc"] = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc", lopHoc.MaKhoaHoc);
            ViewData["MaGiangVien"] = new SelectList(await _context.GiangViens.Where(g => g.TrangThai).OrderBy(g => g.HoTen).ToListAsync(), "MaGiangVien", "HoTen", lopHoc.MaGiangVien);
            return View(lopHoc);
        }

        // =========================================================================
        // TIỆN ÍCH KIỂM TRA TRÙNG PHÒNG HỌC & CA HỌC (AJAX & SERVER-SIDE)
        // =========================================================================

        /// <summary>
        /// Tìm lớp học đang hoạt động bị trùng phòng và ca học
        /// </summary>
        private async Task<LopHoc?> TimLopTrungPhongVaCaHocAsync(int? maLopHienTai, string? phongHoc, string? lichHoc)
        {
            if (string.IsNullOrWhiteSpace(phongHoc) || string.IsNullOrWhiteSpace(lichHoc))
                return null;

            var dsLop = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Where(l => l.TrangThai != "Đã kết thúc" && l.TrangThai != "Đã hủy")
                .ToListAsync();

            foreach (var lop in dsLop)
            {
                if (maLopHienTai.HasValue && lop.MaLop == maLopHienTai.Value)
                    continue;

                if (LichHocHelper.KiemTraTrungPhongVaCaHoc(phongHoc, lichHoc, lop.PhongHoc, lop.LichHoc))
                {
                    return lop;
                }
            }
            return null;
        }

        /// <summary>
        /// Tìm lớp học mà giảng viên đã có lịch dạy trong cùng ca học
        /// </summary>
        private async Task<LopHoc?> TimLopTrungGiangVienVaCaHocAsync(int? maLopHienTai, int maGiangVien, string? lichHoc)
        {
            if (maGiangVien <= 0 || string.IsNullOrWhiteSpace(lichHoc))
                return null;

            var dsLop = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Where(l => l.MaGiangVien == maGiangVien && l.TrangThai != "Đã kết thúc" && l.TrangThai != "Đã hủy")
                .ToListAsync();

            foreach (var lop in dsLop)
            {
                if (maLopHienTai.HasValue && lop.MaLop == maLopHienTai.Value)
                    continue;

                if (LichHocHelper.KiemTraTrungLich(lichHoc, lop.LichHoc))
                {
                    return lop;
                }
            }
            return null;
        }

        /// <summary>
        /// API kiểm tra trùng phòng học & ca học theo thời gian thực (Real-time AJAX Check)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> KiemTraTrungLich(int? maLop, string? phongHoc, string? lichHoc, int? maGiangVien)
        {
            var trungPhong = await TimLopTrungPhongVaCaHocAsync(maLop, phongHoc, lichHoc);
            var trungGv = (maGiangVien.HasValue && maGiangVien.Value > 0)
                ? await TimLopTrungGiangVienVaCaHocAsync(maLop, maGiangVien.Value, lichHoc)
                : null;

            return Json(new
            {
                trungPhong = trungPhong != null,
                thongBaoPhong = trungPhong != null
                    ? $"Cảnh báo: Phòng '{phongHoc}' đã có lớp '{trungPhong.TenLop}' ({trungPhong.KhoaHoc?.TenKhoaHoc}) học vào ca này ({trungPhong.LichHoc})!"
                    : "Phòng học trống và khả dụng cho ca học này.",
                tenLopTrungPhong = trungPhong?.TenLop ?? "",
                trungGiangVien = trungGv != null,
                thongBaoGiangVien = trungGv != null
                    ? $"Cảnh báo: Giảng viên đã có lịch dạy lớp '{trungGv.TenLop}' ({trungGv.LichHoc}) tại phòng {trungGv.PhongHoc}!"
                    : "Giảng viên khả dụng cho ca học này.",
                tenLopTrungGiangVien = trungGv?.TenLop ?? ""
            });
        }

        /// <summary>
        /// Trang hiển thị Thời khóa biểu ma trận Phòng học & Ca học và rà soát toàn bộ các xung đột
        /// </summary>
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> LichPhongHoc(string? phongHoc, string? caHoc)
        {
            var cacLopHoatDong = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Where(l => l.TrangThai != "Đã kết thúc" && l.TrangThai != "Đã hủy")
                .ToListAsync();

            // Danh sách phòng học
            var dsPhong = cacLopHoatDong
                .Select(l => l.PhongHoc.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .Union(LichHocHelper.PhongHocPhoBien)
                .OrderBy(p => p)
                .ToList();

            var dsCa = LichHocHelper.CaHocPhoBien.ToList();

            // Quét toàn bộ để tìm các cặp xung đột
            var danhSachTrung = new List<CanhBaoTrungLichItem>();
            for (int i = 0; i < cacLopHoatDong.Count; i++)
            {
                for (int j = i + 1; j < cacLopHoatDong.Count; j++)
                {
                    var l1 = cacLopHoatDong[i];
                    var l2 = cacLopHoatDong[j];

                    if (LichHocHelper.KiemTraTrungPhongVaCaHoc(l1.PhongHoc, l1.LichHoc, l2.PhongHoc, l2.LichHoc))
                    {
                        danhSachTrung.Add(new CanhBaoTrungLichItem
                        {
                            PhongHoc = l1.PhongHoc,
                            Lop1 = l1,
                            Lop2 = l2,
                            LyDoTrung = $"Trùng phòng '{l1.PhongHoc}': Lớp '{l1.TenLop}' ({l1.LichHoc}) giao lịch với Lớp '{l2.TenLop}' ({l2.LichHoc})"
                        });
                    }
                }
            }

            var vm = new LichPhongHocViewModel
            {
                DanhSachPhongHoc = dsPhong,
                DanhSachCaHoc = dsCa,
                TatCaLopHoatDong = cacLopHoatDong,
                DanhSachTrungLich = danhSachTrung,
                PhongHocLoc = phongHoc,
                CaHocLoc = caHoc,
                TongSoLopDangHoc = cacLopHoatDong.Count,
                TongSoPhongSuDung = cacLopHoatDong.Select(l => LichHocHelper.NormalizeRoom(l.PhongHoc)).Distinct().Count()
            };

            return View(vm);
        }

        // GET: LopHocs/Delete/5 (Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lopHoc = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .FirstOrDefaultAsync(m => m.MaLop == id);
            if (lopHoc == null)
            {
                return NotFound();
            }

            return View(lopHoc);
        }

        // POST: LopHocs/Delete/5 (Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lopHoc = await _context.LopHocs
                .Include(l => l.DangKyHocs)
                .FirstOrDefaultAsync(l => l.MaLop == id);

            if (lopHoc != null)
            {
                if (lopHoc.DangKyHocs != null && lopHoc.DangKyHocs.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa lớp học '{lopHoc.TenLop}' vì đã có {lopHoc.DangKyHocs.Count} học viên đăng ký!";
                    return RedirectToAction(nameof(Index));
                }

                _context.LopHocs.Remove(lopHoc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa lớp học '{lopHoc.TenLop}' thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LopHocExists(int id)
        {
            return _context.LopHocs.Any(e => e.MaLop == id);
        }
    }
}
