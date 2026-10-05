// Họ và tên: Lê Đăng Duy
// Mã sinh viên: 23103100089
// Nội dung thực hiện: Module 4 - Dashboard và thống kê bằng LINQ (Count, Sum, GroupBy, OrderByDescending)

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

            return View(vm);
        }
    }
}