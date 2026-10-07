// Họ và tên: Phạm Văn Công
// Mã sinh viên: 23103100115
// Nội dung thực hiện: Module 1 - TaiKhoanController (Đăng nhập, Đăng ký, Đăng xuất, Profile, Quản lý tài khoản Admin)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Filters;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models.ViewModels;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TaiKhoan/Login
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /TaiKhoan/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var taiKhoan = await _context.TaiKhoans
                    .Include(t => t.HocVien)
                    .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap && t.MatKhau == model.MatKhau);

                if (taiKhoan == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                    return View(model);
                }

                if (!taiKhoan.TrangThai)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn hiện đang bị khóa. Vui lòng liên hệ Admin.");
                    return View(model);
                }

                // Lưu Session người dùng
                HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
                HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap);
                HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
                HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);

                if (taiKhoan.HocVien != null)
                {
                    HttpContext.Session.SetInt32("MaHocVien", taiKhoan.HocVien.MaHocVien);
                }

                TempData["SuccessMessage"] = $"Đăng nhập thành công! Chào mừng {taiKhoan.HoTen}.";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // GET: /TaiKhoan/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /TaiKhoan/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool tonTaiTenDangNhap = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap);
                if (tonTaiTenDangNhap)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã được sử dụng.");
                    return View(model);
                }

                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = model.MatKhau,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    VaiTro = "HocVien",
                    TrangThai = true
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                var hocVien = new HocVien
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = model.HoTen,
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    GioiTinh = model.GioiTinh ?? "Nam",
                    NgaySinh = model.NgaySinh ?? DateTime.Now.AddYears(-20),
                    DiaChi = model.DiaChi ?? ""
                };

                _context.HocViens.Add(hocVien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }
            return View(model);
        }

        // GET: /TaiKhoan/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Bạn đã đăng xuất khỏi hệ thống.";
            return RedirectToAction("Login");
        }

        // GET: /TaiKhoan/Profile
        [AuthorizeRole("Admin", "HocVien")]
        public async Task<IActionResult> Profile()
        {
            int? maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (!maTaiKhoan.HasValue) return RedirectToAction(nameof(Login));

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.HocVien)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // GET: /TaiKhoan/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /TaiKhoan/Index (Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Index()
        {
            var dsTaiKhoan = await _context.TaiKhoans.OrderByDescending(t => t.MaTaiKhoan).ToListAsync();
            return View(dsTaiKhoan);
        }
    }
}
