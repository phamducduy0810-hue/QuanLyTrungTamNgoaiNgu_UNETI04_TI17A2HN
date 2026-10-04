// Họ và tên: Lê Đăng Duy
// Mã sinh viên: 23103100089
// Nội dung thực hiện: Module 4 - Quản lý đăng ký: xem, chi tiết, tìm kiếm, lọc, xác nhận, hủy, cập nhật trạng thái

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models.ViewModels;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class QuanLyDangKyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyDangKyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Admin ở phía Server cho MỌI action của controller này
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

        // GET: /QuanLyDangKy?tuKhoa=...&maLop=...&trangThai=...
        public async Task<IActionResult> Index(string? tuKhoa, int? maLop, string? trangThai)
        {
            // Truy vấn -> Tìm kiếm -> Lọc -> Sắp xếp -> Hiển thị
            var query = _context.DangKyHocs
                .Include(d => d.HocVien)
                .Include(d => d.LopHoc)
                    .ThenInclude(l => l!.KhoaHoc)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(d => d.HocVien!.HoTen.Contains(tuKhoa));
            }

            if (maLop.HasValue)
            {
                query = query.Where(d => d.MaLop == maLop.Value);
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            var vm = new DangKyIndexViewModel
            {
                DanhSach = await query
                    .OrderByDescending(d => d.NgayDangKy)
                    .ThenByDescending(d => d.MaDangKy)
                    .ToListAsync(),
                TuKhoa = tuKhoa,
                MaLop = maLop,
                TrangThai = trangThai,
                DanhSachLop = await _context.LopHocs
                    .OrderBy(l => l.TenLop)
                    .Select(l => new SelectListItem { Value = l.MaLop.ToString(), Text = l.TenLop })
                    .ToListAsync(),
                DanhSachTrangThai = TrangThaiDangKy.TatCa.ToList()
            };

            return View(vm);
        }

        // GET: /QuanLyDangKy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dangKy = await _context.DangKyHocs
                .Include(d => d.HocVien)
                .Include(d => d.LopHoc)
                    .ThenInclude(l => l!.KhoaHoc)
                .FirstOrDefaultAsync(d => d.MaDangKy == id);

            if (dangKy == null) return NotFound();

            // Số học viên đã xác nhận của lớp, để hiển thị "còn chỗ / đã đủ"
            ViewBag.SoDaXacNhan = await _context.DangKyHocs
                .CountAsync(d => d.MaLop == dangKy.MaLop && d.TrangThai == TrangThaiDangKy.DaXacNhan);
            ViewBag.DanhSachTrangThai = TrangThaiDangKy.TatCa;

            return View(dangKy);
        }

        // POST: xác nhận đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id, string? quayLai)
        {
            var kq = await DoiTrangThai(id, TrangThaiDangKy.DaXacNhan);
            return KetQua(id, kq, quayLai);
        }

        // POST: hủy đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id, string? quayLai)
        {
            var kq = await DoiTrangThai(id, TrangThaiDangKy.DaHuy);
            return KetQua(id, kq, quayLai);
        }

        // POST: cập nhật trạng thái tùy chọn (chọn từ select ở trang chi tiết)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThai)
        {
            var kq = await DoiTrangThai(id, trangThai);
            return KetQua(id, kq, "Details");
        }

        // Đặt thông báo TempData và quay lại trang phù hợp
        private IActionResult KetQua(int id, (bool ThanhCong, string ThongBao) kq, string? quayLai)
        {
            TempData[kq.ThanhCong ? "ThanhCong" : "Loi"] = kq.ThongBao;
            if (quayLai == "Details")
                return RedirectToAction(nameof(Details), new { id });
            return RedirectToAction(nameof(Index));
        }

        // Nghiệp vụ chung: đổi trạng thái đăng ký, có kiểm tra sĩ số khi xác nhận
        private async Task<(bool ThanhCong, string ThongBao)> DoiTrangThai(int id, string trangThaiMoi)
        {
            if (!TrangThaiDangKy.TatCa.Contains(trangThaiMoi))
                return (false, "Trạng thái không hợp lệ.");

            var dangKy = await _context.DangKyHocs
                .Include(d => d.LopHoc)
                .FirstOrDefaultAsync(d => d.MaDangKy == id);

            if (dangKy == null)
                return (false, "Không tìm thấy đăng ký.");

            if (dangKy.TrangThai == trangThaiMoi)
                return (false, $"Đăng ký đã ở trạng thái \"{trangThaiMoi}\".");

            if (trangThaiMoi == TrangThaiDangKy.DaXacNhan)
            {
                var lop = dangKy.LopHoc;
                if (lop == null)
                    return (false, "Không tìm thấy lớp học của đăng ký này.");

                // Đếm số đăng ký ĐÃ XÁC NHẬN của lớp (đăng ký hiện tại chưa được tính vì chưa đổi trạng thái)
                int soDaXacNhan = await _context.DangKyHocs
                    .CountAsync(d => d.MaLop == dangKy.MaLop && d.TrangThai == TrangThaiDangKy.DaXacNhan);

                if (soDaXacNhan >= lop.SoLuongToiDa)
                    return (false, $"Lớp \"{lop.TenLop}\" đã đủ số lượng ({soDaXacNhan}/{lop.SoLuongToiDa}), không thể xác nhận thêm.");
            }

            dangKy.TrangThai = trangThaiMoi;
            await _context.SaveChangesAsync();
            return (true, $"Đã cập nhật đăng ký #{id} sang trạng thái \"{trangThaiMoi}\".");
        }
    }
}