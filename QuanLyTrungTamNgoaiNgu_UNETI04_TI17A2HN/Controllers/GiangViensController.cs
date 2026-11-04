// Họ và tên: Diêm Đức Hùng
// Mã sinh viên: 23103100099
// Nội dung thực hiện: Module 3 - Quản lý giảng viên, Phân công giảng dạy lớp học
   
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Helpers;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class GiangViensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GiangViensController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("VaiTro") == "Admin";
        }

        // ==========================================
        // 1. QUẢN LÝ GIẢNG VIÊN (CRUD)
        // ==========================================

        // GET: /GiangViens
        public async Task<IActionResult> Index(string? tuKhoa, string? chuyenMon, string? trinhDo, bool? trangThai)
        {
            if (!IsAdmin())
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            var query = _context.GiangViens.Include(g => g.LopHocs).AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(g => g.HoTen.Contains(tuKhoa) || 
                                         g.Email.Contains(tuKhoa) || 
                                         g.SoDienThoai.Contains(tuKhoa) || 
                                         (g.ChuyenMon != null && g.ChuyenMon.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(chuyenMon))
            {
                query = query.Where(g => g.ChuyenMon != null && g.ChuyenMon.Contains(chuyenMon));
            }

            if (!string.IsNullOrWhiteSpace(trinhDo))
            {
                query = query.Where(g => g.TrinhDo != null && g.TrinhDo.Contains(trinhDo));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(g => g.TrangThai == trangThai.Value);
            }

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.ChuyenMon = chuyenMon;
            ViewBag.TrinhDo = trinhDo;
            ViewBag.TrangThai = trangThai;

            // Thống kê nhanh
            ViewBag.TongSoGV = await _context.GiangViens.CountAsync();
            ViewBag.GVDangDay = await _context.GiangViens.CountAsync(g => g.TrangThai);
            ViewBag.TongLopDaPhanCong = await _context.LopHocs.CountAsync(l => l.MaGiangVien != null);
            ViewBag.TongLopChuaPhanCong = await _context.LopHocs.CountAsync(l => l.MaGiangVien == null);

            var danhSach = await query.OrderByDescending(g => g.MaGiangVien).ToListAsync();
            return View(danhSach);
        }

        // GET: /GiangViens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin())
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var giangVien = await _context.GiangViens
                .Include(g => g.LopHocs)
                    .ThenInclude(l => l.KhoaHoc)
                .Include(g => g.LopHocs)
                    .ThenInclude(l => l.DangKyHocs)
                .FirstOrDefaultAsync(m => m.MaGiangVien == id);

            if (giangVien == null) return NotFound();

            // Danh sách các lớp chưa được phân công để có thể gán nhanh ngay trong trang Details
            ViewBag.LopChuaPhanCong = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Where(l => l.MaGiangVien == null)
                .OrderBy(l => l.TenLop)
                .ToListAsync();

            return View(giangVien);
        }

        // GET: /GiangViens/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            return View(new GiangVien { TrangThai = true, GioiTinh = "Nam", NgaySinh = DateTime.Now.AddYears(-28) });
        }

        // POST: /GiangViens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,TrinhDo,ChuyenMon,DiaChi,GhiChu,TrangThai")] GiangVien giangVien)
        {
            if (!IsAdmin()) return Unauthorized();

            if (await _context.GiangViens.AnyAsync(g => g.Email == giangVien.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký cho một giảng viên khác.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(giangVien);
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = $"Thêm mới giảng viên '{giangVien.HoTen}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(giangVien);
        }

        // GET: /GiangViens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var giangVien = await _context.GiangViens.FindAsync(id);
            if (giangVien == null) return NotFound();

            return View(giangVien);
        }

        // POST: /GiangViens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaGiangVien,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,TrinhDo,ChuyenMon,DiaChi,GhiChu,TrangThai")] GiangVien giangVien)
        {
            if (!IsAdmin()) return Unauthorized();

            if (id != giangVien.MaGiangVien) return NotFound();

            if (await _context.GiangViens.AnyAsync(g => g.Email == giangVien.Email && g.MaGiangVien != id))
            {
                ModelState.AddModelError("Email", "Email này đã thuộc về giảng viên khác.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(giangVien);
                    await _context.SaveChangesAsync();
                    TempData["ThanhCong"] = $"Cập nhật giảng viên '{giangVien.HoTen}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiangVienExists(giangVien.MaGiangVien))
                    {
                        return NotFound();
                    }
                    else throw;
                }
            }

            return View(giangVien);
        }

        // GET: /GiangViens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var giangVien = await _context.GiangViens
                .Include(g => g.LopHocs)
                .FirstOrDefaultAsync(m => m.MaGiangVien == id);

            if (giangVien == null) return NotFound();

            return View(giangVien);
        }

        // POST: /GiangViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var giangVien = await _context.GiangViens
                .Include(g => g.LopHocs)
                .FirstOrDefaultAsync(g => g.MaGiangVien == id);

            if (giangVien != null)
            {
                // Hủy phân công các lớp trước khi xóa giảng viên
                if (giangVien.LopHocs != null)
                {
                    foreach (var lop in giangVien.LopHocs)
                    {
                        lop.MaGiangVien = null;
                    }
                }

                _context.GiangViens.Remove(giangVien);
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = $"Đã xóa giảng viên '{giangVien.HoTen}' thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 2. PHÂN CÔNG LỚP HỌC CHO GIẢNG VIÊN
        // ==========================================

        // GET: /GiangViens/PhanCong
        public async Task<IActionResult> PhanCong(int? maGiangVien, int? maKhoaHoc, string? trangThaiPhanCong, string? tuKhoa)
        {
            if (!IsAdmin())
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            var queryLop = _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                queryLop = queryLop.Where(l => l.TenLop.Contains(tuKhoa) || 
                                               l.PhongHoc.Contains(tuKhoa) || 
                                               (l.GiangVien != null && l.GiangVien.HoTen.Contains(tuKhoa)));
            }

            if (maKhoaHoc.HasValue)
            {
                queryLop = queryLop.Where(l => l.MaKhoaHoc == maKhoaHoc.Value);
            }

            if (maGiangVien.HasValue)
            {
                queryLop = queryLop.Where(l => l.MaGiangVien == maGiangVien.Value);
            }

            if (trangThaiPhanCong == "DaPhanCong")
            {
                queryLop = queryLop.Where(l => l.MaGiangVien != null);
            }
            else if (trangThaiPhanCong == "ChuaPhanCong")
            {
                queryLop = queryLop.Where(l => l.MaGiangVien == null);
            }

            var dsLop = await queryLop.OrderBy(l => l.MaGiangVien.HasValue ? 1 : 0).ThenBy(l => l.TenLop).ToListAsync();
            var dsGiangVien = await _context.GiangViens.Where(g => g.TrangThai).OrderBy(g => g.HoTen).ToListAsync();
            var dsKhoaHoc = await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync();

            GiangVien? gvChon = null;
            if (maGiangVien.HasValue)
            {
                gvChon = await _context.GiangViens.Include(g => g.LopHocs).FirstOrDefaultAsync(g => g.MaGiangVien == maGiangVien.Value);
            }

            var viewModel = new PhanCongLopViewModel
            {
                GiangVienChon = gvChon,
                DanhSachLop = dsLop,
                DanhSachGiangVien = dsGiangVien,
                DanhSachKhoaHoc = dsKhoaHoc,
                TuKhoa = tuKhoa,
                MaKhoaHoc = maKhoaHoc,
                MaGiangVien = maGiangVien,
                TrangThaiPhanCong = trangThaiPhanCong
            };

            return View(viewModel);
        }

        // POST: /GiangViens/CapNhatPhanCong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatPhanCong(int maLop, int? maGiangVien, string? returnUrl)
        {
            if (!IsAdmin()) return Unauthorized();

            var lop = await _context.LopHocs.Include(l => l.KhoaHoc).FirstOrDefaultAsync(l => l.MaLop == maLop);
            if (lop == null) return NotFound();

            if (maGiangVien.HasValue && maGiangVien.Value > 0)
            {
                var gv = await _context.GiangViens.FindAsync(maGiangVien.Value);
                if (gv == null)
                {
                    TempData["Loi"] = "Giảng viên được chọn không tồn tại.";
                }
                else
                {
                    // Kiểm tra trùng lịch giảng dạy của giảng viên vào cùng ca học
                    var lopTrungLich = await _context.LopHocs
                        .Where(l => l.MaLop != maLop && l.MaGiangVien == maGiangVien.Value && l.TrangThai != "Đã kết thúc" && l.TrangThai != "Đã hủy")
                        .ToListAsync();

                    var lopXungDot = lopTrungLich.FirstOrDefault(l => LichHocHelper.KiemTraTrungLich(l.LichHoc, lop.LichHoc));
                    if (lopXungDot != null)
                    {
                        TempData["Loi"] = $"Trùng ca dạy! Giảng viên '{gv.HoTen}' đã có lịch dạy lớp '{lopXungDot.TenLop}' ({lopXungDot.LichHoc}) tại phòng {lopXungDot.PhongHoc}. Vui lòng chọn giảng viên khác!";
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction(nameof(PhanCong), new { maGiangVien });
                    }

                    lop.MaGiangVien = maGiangVien.Value;
                    await _context.SaveChangesAsync();
                    TempData["ThanhCong"] = $"Đã phân công giảng viên '{gv.HoTen}' phụ trách lớp '{lop.TenLop}' thành công!";
                }
            }
            else
            {
                lop.MaGiangVien = null;
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = $"Đã hủy phân công giảng viên cho lớp '{lop.TenLop}'!";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(PhanCong), new { maGiangVien });
        }

        // POST: /GiangViens/HuyPhanCong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhanCong(int maLop, string? returnUrl)
        {
            if (!IsAdmin()) return Unauthorized();

            var lop = await _context.LopHocs.FindAsync(maLop);
            if (lop != null)
            {
                lop.MaGiangVien = null;
                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = $"Đã hủy phân công giảng viên cho lớp '{lop.TenLop}'!";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(PhanCong));
        }

        // ==========================================
        // 3. CHỨC NĂNG DÀNH RIÊNG CHO GIẢNG VIÊN ĐĂNG NHẬP
        // ==========================================

        // Helper lấy Mã Giảng Viên hiện tại từ Session hoặc Tài khoản
        private async Task<GiangVien?> GetGiangVienHienTaiAsync()
        {
            var maGiangVien = HttpContext.Session.GetInt32("MaGiangVien");
            if (maGiangVien.HasValue)
            {
                return await _context.GiangViens.FindAsync(maGiangVien.Value);
            }

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan.HasValue)
            {
                return await _context.GiangViens.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan.Value);
            }

            return null;
        }

        // GET: /GiangViens/LichDay (Xem lịch dạy & lớp học được phân công của Giảng viên)
        public async Task<IActionResult> LichDay()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "GiangVien" && vaiTro != "Admin")
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập với tài khoản Giảng viên.";
                return RedirectToAction("Login", "TaiKhoan");
            }

            var gv = await GetGiangVienHienTaiAsync();
            if (gv == null)
            {
                TempData["Loi"] = "Không tìm thấy hồ sơ giảng viên gắn với tài khoản này.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy tất cả lớp học mà giảng viên này đang phụ trách
            var danhSachLop = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.DangKyHocs)
                .Where(l => l.MaGiangVien == gv.MaGiangVien)
                .OrderBy(l => l.NgayKhaiGiang)
                .ToListAsync();

            ViewBag.GiangVien = gv;
            return View(danhSachLop);
        }

        // GET: /GiangViens/DanhSachHocVien/5 (Xem danh sách học viên trong lớp của giảng viên)
        public async Task<IActionResult> DanhSachHocVien(int? id)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "GiangVien" && vaiTro != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Login", "TaiKhoan");
            }

            if (id == null) return NotFound();

            var gv = await GetGiangVienHienTaiAsync();
            var lopHoc = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.GiangVien)
                .Include(l => l.DangKyHocs)
                    .ThenInclude(d => d.HocVien)
                .FirstOrDefaultAsync(l => l.MaLop == id);

            if (lopHoc == null) return NotFound();

            // Nếu là Giảng viên thì chỉ được xem lớp của chính mình
            if (vaiTro == "GiangVien" && gv != null && lopHoc.MaGiangVien != gv.MaGiangVien)
            {
                TempData["Loi"] = "Bạn chỉ có thể xem danh sách học viên của lớp do bạn phụ trách giảng dạy.";
                return RedirectToAction(nameof(LichDay));
            }

            return View(lopHoc);
        }

        // GET: /GiangViens/HoSoCaNhan (Giảng viên cập nhật hồ sơ cá nhân)
        public async Task<IActionResult> HoSoCaNhan()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "GiangVien")
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var gv = await GetGiangVienHienTaiAsync();
            if (gv == null)
            {
                TempData["Loi"] = "Chưa có thông tin giảng viên gắn với tài khoản này.";
                return RedirectToAction("Index", "Home");
            }

            return View(gv);
        }

        // POST: /GiangViens/HoSoCaNhan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoSoCaNhan([Bind("MaGiangVien,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,TrinhDo,ChuyenMon,DiaChi,GhiChu")] GiangVien model)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "GiangVien") return RedirectToAction("Login", "TaiKhoan");

            var gv = await GetGiangVienHienTaiAsync();
            if (gv == null || gv.MaGiangVien != model.MaGiangVien) return Unauthorized();

            if (ModelState.IsValid)
            {
                gv.HoTen = model.HoTen;
                gv.NgaySinh = model.NgaySinh;
                gv.GioiTinh = model.GioiTinh;
                gv.SoDienThoai = model.SoDienThoai;
                gv.Email = model.Email;
                gv.TrinhDo = model.TrinhDo;
                gv.ChuyenMon = model.ChuyenMon;
                gv.DiaChi = model.DiaChi;
                gv.GhiChu = model.GhiChu;

                await _context.SaveChangesAsync();
                TempData["ThanhCong"] = "Cập nhật hồ sơ cá nhân thành công!";
                return RedirectToAction(nameof(HoSoCaNhan));
            }

            return View(model);
        }

        private bool GiangVienExists(int id)
        {
            return _context.GiangViens.Any(e => e.MaGiangVien == id);
        }
    }
}
