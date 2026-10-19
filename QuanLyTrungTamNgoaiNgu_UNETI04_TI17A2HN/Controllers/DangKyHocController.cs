// Họ và tên: Diem Duc Hung
// Mã sinh viên: 23103100099
// Nội dung thực hiện: Module 3 - Đăng ký lớp học, Lịch sử đăng ký   

using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class DangKyHocController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DangKyHocController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DangKyHoc/DangKy/5
        public async Task<IActionResult> DangKy(int? maLop)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập hoặc đăng ký tài khoản để tiếp tục đăng ký lớp học.";
                string returnUrl = Url.Action("DangKy", "DangKyHoc", new { maLop = maLop }) ?? "/";
                return RedirectToAction("Login", "TaiKhoan", new { returnUrl = returnUrl });
            }

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null) return RedirectToAction("Login", "TaiKhoan");

            var hocVien = await _context.HocViens.FirstOrDefaultAsync(h => h.MaTaiKhoan == maTaiKhoan);
            if (hocVien == null)
            {
                var tk = await _context.TaiKhoans.FindAsync(maTaiKhoan);
                if (tk != null)
                {
                    hocVien = new HocVien
                    {
                        MaTaiKhoan = tk.MaTaiKhoan,
                        HoTen = tk.HoTen,
                        Email = tk.Email,
                        SoDienThoai = "0988888888",
                        GioiTinh = "Nam",
                        NgaySinh = DateTime.Today.AddYears(-20),
                        DiaChi = "Hà Nội"
                    };
                    _context.HocViens.Add(hocVien);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["Loi"] = "Bạn cần cập nhật thông tin cá nhân trước khi đăng ký.";
                    return RedirectToAction("CaNhan", "HocViens");
                }
            }

            if (maLop == null) return NotFound();

            var lopHoc = await _context.LopHocs.Include(l => l.KhoaHoc).FirstOrDefaultAsync(l => l.MaLop == maLop);
            if (lopHoc == null) return NotFound();

            // Kiểm tra điều kiện
            if (lopHoc.TrangThai != "Đang tuyển sinh" && lopHoc.TrangThai != "Sắp mở")
            {
                TempData["Loi"] = "Lớp học không còn tuyển sinh.";
                return RedirectToAction("Details", "LopHocs", new { id = maLop });
            }

            bool daDangKy = await _context.DangKyHocs.AnyAsync(d => d.MaHocVien == hocVien.MaHocVien && d.MaLop == maLop);
            if (daDangKy)
            {
                TempData["Loi"] = "Bạn đã đăng ký lớp học này rồi.";
                return RedirectToAction("Details", "LopHocs", new { id = maLop });
            }

            int soDaXacNhan = await _context.DangKyHocs.CountAsync(d => d.MaLop == maLop && d.TrangThai == "Đã xác nhận");
            if (soDaXacNhan >= lopHoc.SoLuongToiDa)
            {
                TempData["Loi"] = "Lớp học đã đủ số lượng.";
                return RedirectToAction("Details", "LopHocs", new { id = maLop });
            }

            ViewBag.LopHoc = lopHoc;
            return View(hocVien);
        }

        // POST: DangKyHoc/DangKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDangKy(int maLop)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro)) return RedirectToAction("Login", "TaiKhoan");

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null) return RedirectToAction("Login", "TaiKhoan");

            var hocVien = await _context.HocViens.FirstOrDefaultAsync(h => h.MaTaiKhoan == maTaiKhoan);
            if (hocVien == null)
            {
                var tk = await _context.TaiKhoans.FindAsync(maTaiKhoan);
                if (tk != null)
                {
                    hocVien = new HocVien
                    {
                        MaTaiKhoan = tk.MaTaiKhoan,
                        HoTen = tk.HoTen,
                        Email = tk.Email,
                        SoDienThoai = "0988888888",
                        GioiTinh = "Nam",
                        NgaySinh = DateTime.Today.AddYears(-20),
                        DiaChi = "Hà Nội"
                    };
                    _context.HocViens.Add(hocVien);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return RedirectToAction("CaNhan", "HocViens");
                }
            }

            var lopHoc = await _context.LopHocs.FirstOrDefaultAsync(l => l.MaLop == maLop);
            if (lopHoc == null) return NotFound();

            // Kiểm tra điều kiện lần nữa (tránh đăng ký đồng thời)
            if (lopHoc.TrangThai != "Đang tuyển sinh" && lopHoc.TrangThai != "Sắp mở")
            {
                TempData["Loi"] = "Lớp học không còn tuyển sinh.";
                return RedirectToAction("Details", "LopHocs", new { id = maLop });
            }

            bool daDangKy = await _context.DangKyHocs.AnyAsync(d => d.MaHocVien == hocVien.MaHocVien && d.MaLop == maLop);
            if (daDangKy)
            {
                TempData["Loi"] = "Bạn đã đăng ký lớp học này rồi.";
                return RedirectToAction("Details", "LopHocs", new { id = maLop });
            }

            int soDaXacNhan = await _context.DangKyHocs.CountAsync(d => d.MaLop == maLop && d.TrangThai == "Đã xác nhận");
            if (soDaXacNhan >= lopHoc.SoLuongToiDa)
            {
                TempData["Loi"] = "Lớp học đã đủ số lượng.";
                return RedirectToAction("Details", "LopHocs", new { id = maLop });
            }

            var dangKyMoi = new DangKyHoc
            {
                MaHocVien = hocVien.MaHocVien,
                MaLop = lopHoc.MaLop,
                NgayDangKy = DateTime.Now,
                HocPhi = lopHoc.HocPhi,
                TrangThai = "Chờ xác nhận"
            };

            _context.DangKyHocs.Add(dangKyMoi);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = "Đăng ký thành công! Vui lòng chờ admin xác nhận.";
            return RedirectToAction(nameof(LichSu));
        }

        // GET: DangKyHoc/LichSu
        public async Task<IActionResult> LichSu()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro)) return RedirectToAction("Login", "TaiKhoan");

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null) return RedirectToAction("Login", "TaiKhoan");

            var hocVien = await _context.HocViens.FirstOrDefaultAsync(h => h.MaTaiKhoan == maTaiKhoan);
            if (hocVien == null)
            {
                return View(new List<DangKyHoc>());
            }

            var danhSach = await _context.DangKyHocs
                .Include(d => d.LopHoc)
                .ThenInclude(l => l.KhoaHoc)
                .Where(d => d.MaHocVien == hocVien.MaHocVien)
                .OrderByDescending(d => d.NgayDangKy)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: DangKyHoc/LichSuDangKy (Hỗ trợ URL alias)
        [HttpGet]
        public Task<IActionResult> LichSuDangKy() => LichSu();

        // GET: DangKyHoc/DangKyClass (Hỗ trợ URL alias)
        [HttpGet]
        public Task<IActionResult> DangKyClass(int? id, int? maLop) => DangKy(maLop ?? id);
    }
}
