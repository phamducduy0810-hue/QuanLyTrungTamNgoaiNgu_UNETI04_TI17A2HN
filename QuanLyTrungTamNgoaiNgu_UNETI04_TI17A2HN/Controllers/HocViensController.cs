// Họ và tên: Diem Duc Hung
// Mã sinh viên: 23103100099
// Nội dung thực hiện: Module 3 - Quản lý học viên, Xem thông tin cá nhân, Cập nhật thông tin

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class HocViensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HocViensController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Danh sách học viên
        public async Task<IActionResult> Index(string? tuKhoa)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin")
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            var query = _context.HocViens.Include(h => h.TaiKhoan).AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(h => h.HoTen.Contains(tuKhoa) || (h.Email != null && h.Email.Contains(tuKhoa)) || (h.SoDienThoai != null && h.SoDienThoai.Contains(tuKhoa)));
            }

            ViewBag.TuKhoa = tuKhoa;
            return View(await query.ToListAsync());
        }

        // GET: Admin/Chi tiết học viên
        public async Task<IActionResult> Details(int? id)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "Admin")
            {
                TempData["Loi"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var hocVien = await _context.HocViens
                .Include(h => h.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaHocVien == id);
            
            if (hocVien == null) return NotFound();

            return View(hocVien);
        }

        // GET: Học viên/Thông tin cá nhân
        public async Task<IActionResult> CaNhan()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "HocVien") return RedirectToAction("Login", "TaiKhoan");

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null) return RedirectToAction("Login", "TaiKhoan");

            var hocVien = await _context.HocViens
                .Include(h => h.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaTaiKhoan == maTaiKhoan);

            if (hocVien == null)
            {
                // Nếu chưa có thông tin học viên thì tạo mới rỗng để cập nhật
                hocVien = new HocVien { MaTaiKhoan = maTaiKhoan.Value, NgaySinh = DateTime.Now };
                var tk = await _context.TaiKhoans.FindAsync(maTaiKhoan);
                if (tk != null)
                {
                    hocVien.HoTen = tk.HoTen;
                    hocVien.Email = tk.Email;
                    hocVien.SoDienThoai = "";
                    hocVien.GioiTinh = "";
                    hocVien.DiaChi = "";
                }
                else
                {
                    hocVien.HoTen = "Chưa có tên";
                    hocVien.Email = "chuaco@email.com";
                    hocVien.SoDienThoai = "0123456789";
                    hocVien.GioiTinh = "Nam";
                    hocVien.DiaChi = "";
                }
                _context.Add(hocVien);
                await _context.SaveChangesAsync();
            }

            return View(hocVien);
        }

        // POST: Học viên/Cập nhật thông tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaNhan([Bind("MaHocVien,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi")] HocVien hocVien)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "HocVien") return RedirectToAction("Login", "TaiKhoan");

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null || hocVien.MaTaiKhoan != maTaiKhoan) return Unauthorized();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hocVien);
                    await _context.SaveChangesAsync();
                    TempData["ThanhCong"] = "Cập nhật thông tin cá nhân thành công!";
                    return RedirectToAction(nameof(CaNhan));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HocVienExists(hocVien.MaHocVien))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(hocVien);
        }

        private bool HocVienExists(int id)
        {
            return _context.HocViens.Any(e => e.MaHocVien == id);
        }
    }
}
