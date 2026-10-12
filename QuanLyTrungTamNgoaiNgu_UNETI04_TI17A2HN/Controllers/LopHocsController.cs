// Họ và tên: Phạm Đức Duy
// Mã sinh viên: 23103100084
// Nội dung thực hiện: Module 2 - Quản lý lớp học, tìm kiếm, lọc, sắp xếp và phân trang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Filters;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class LopHocsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LopHocsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LopHocs
        public async Task<IActionResult> Index(
            string? searchKeyword,
            int? maKhoaHoc,
            string? ngoaiNgu,
            string? trangThai,
            decimal? giaTu,
            decimal? giaDen,
            string? sortOrder,
            int currentPage = 1,
            int? page = null)
        {
            int pageSize = 5;
            int p = page ?? currentPage;
            if (p < 1) p = 1;

            var query = _context.LopHocs.Include(l => l.KhoaHoc).AsQueryable();

            // 1. Tìm kiếm theo Tên lớp hoặc Tên khóa học (LINQ)
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim();
                query = query.Where(l => l.TenLop.Contains(kw) || (l.KhoaHoc != null && l.KhoaHoc.TenKhoaHoc.Contains(kw)));
            }

            // 2. Lọc theo Khóa học
            if (maKhoaHoc.HasValue)
            {
                query = query.Where(l => l.MaKhoaHoc == maKhoaHoc.Value);
            }

            // 3. Lọc theo Ngoại ngữ
            if (!string.IsNullOrWhiteSpace(ngoaiNgu))
            {
                query = query.Where(l => l.KhoaHoc != null && l.KhoaHoc.NgoaiNgu == ngoaiNgu);
            }

            // 4. Lọc theo Trạng thái lớp
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(l => l.TrangThai == trangThai);
            }

            // 5. Lọc theo Khoảng học phí (Từ - Đến)
            if (giaTu.HasValue)
            {
                query = query.Where(l => l.HocPhi >= giaTu.Value);
            }
            if (giaDen.HasValue)
            {
                query = query.Where(l => l.HocPhi <= giaDen.Value);
            }

            // 6. Sắp xếp (6 tiêu chí: Tên A-Z, Tên Z-A, Học phí thấp-cao, Học phí cao-thấp, Khai giảng tăng, Khai giảng giảm)
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
                GiaTu = giaTu,
                GiaDen = giaDen,
                SortOrder = sortOrder ?? "name_asc",
                CurrentPage = p,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            // Dữ liệu dropdown lọc
            ViewBag.KhoaHocList = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc", maKhoaHoc);
            ViewBag.NgoaiNguList = new SelectList(await _context.KhoaHocs.Where(k => !string.IsNullOrEmpty(k.NgoaiNgu)).Select(k => k.NgoaiNgu).Distinct().ToListAsync(), ngoaiNgu);
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
                .Include(l => l.DangKyHocs)
                .FirstOrDefaultAsync(m => m.MaLop == id);
            if (lopHoc == null)
            {
                return NotFound();
            }

            return View(lopHoc);
        }

        // GET: LopHocs/Create (Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create()
        {
            ViewData["MaKhoaHoc"] = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc");
            var model = new LopHoc
            {
                NgayKhaiGiang = DateTime.Today.AddDays(7),
                TrangThai = "Đang tuyển sinh",
                SoLuongToiDa = 25,
                HocPhi = 1000000
            };
            return View(model);
        }

        // POST: LopHocs/Create (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create([Bind("MaLop,TenLop,MaKhoaHoc,NgayKhaiGiang,LichHoc,PhongHoc,SoLuongToiDa,HocPhi,TrangThai")] LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lopHoc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Thêm mới lớp học '{lopHoc.TenLop}' thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKhoaHoc"] = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc", lopHoc.MaKhoaHoc);
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
            return View(lopHoc);
        }

        // POST: LopHocs/Edit/5 (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MaLop,TenLop,MaKhoaHoc,NgayKhaiGiang,LichHoc,PhongHoc,SoLuongToiDa,HocPhi,TrangThai")] LopHoc lopHoc)
        {
            if (id != lopHoc.MaLop)
            {
                return NotFound();
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
            ViewData["MaKhoaHoc"] = new SelectList(await _context.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync(), "MaKhoaHoc", "TenKhoaHoc", lopHoc.MaKhoaHoc);
            return View(lopHoc);
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
