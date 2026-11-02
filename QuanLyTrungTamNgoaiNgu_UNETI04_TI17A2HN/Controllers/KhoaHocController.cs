// Họ và tên: Phạm Văn Công
// Mã sinh viên: 23103100115
// Nội dung thực hiện: Module 1 - KhoaHocController (CRUD Quản lý Khóa học & Chi tiết Khóa học)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Filters;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class KhoaHocController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhoaHocController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KhoaHoc
        public async Task<IActionResult> Index(
            string? searchNgoaiNgu, 
            string? searchCapDo, 
            string? searchKeyword,
            string? giaTu,
            string? giaDen,
            bool? trangThai,
            string? sortOrder)
        {
            IQueryable<KhoaHoc> query = _context.KhoaHocs.Include(k => k.LopHocs);

            // 1. Lọc theo Ngoại ngữ
            if (!string.IsNullOrWhiteSpace(searchNgoaiNgu))
            {
                string nn = searchNgoaiNgu.Trim();
                query = query.Where(k => k.NgoaiNgu == nn || k.NgoaiNgu.Contains(nn));
            }

            // 2. Lọc theo Cấp độ đào tạo
            if (!string.IsNullOrWhiteSpace(searchCapDo))
            {
                string cd = searchCapDo.Trim();
                query = query.Where(k => k.CapDo == cd);
            }

            // 3. Tìm kiếm theo từ khóa (Tên khóa học hoặc Mô tả)
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim().ToLower();
                query = query.Where(k => k.TenKhoaHoc.ToLower().Contains(kw) || 
                                        (k.MoTa != null && k.MoTa.ToLower().Contains(kw)));
            }

            // 4. Lọc theo Khoảng học phí (Từ - Đến) với parser hỗ trợ đa định dạng
            decimal? minPrice = LopHocsController.ParseFee(giaTu);
            decimal? maxPrice = LopHocsController.ParseFee(giaDen);

            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(k => k.HocPhi >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(k => k.HocPhi <= maxPrice.Value);
            }

            // 5. Lọc theo Trạng thái khóa học
            if (trangThai.HasValue)
            {
                query = query.Where(k => k.TrangThai == trangThai.Value);
            }

            // 6. Sắp xếp danh sách
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(k => k.HocPhi);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(k => k.HocPhi);
                    break;
                case "name_asc":
                    query = query.OrderBy(k => k.TenKhoaHoc);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(k => k.TenKhoaHoc);
                    break;
                case "sessions_desc":
                    query = query.OrderByDescending(k => k.SoBuoi);
                    break;
                case "sessions_asc":
                    query = query.OrderBy(k => k.SoBuoi);
                    break;
                default:
                    query = query.OrderByDescending(k => k.MaKhoaHoc);
                    break;
            }

            ViewBag.SearchNgoaiNgu = searchNgoaiNgu;
            ViewBag.SearchCapDo = searchCapDo;
            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.GiaTu = minPrice;
            ViewBag.GiaDen = maxPrice;
            ViewBag.TrangThai = trangThai;
            ViewBag.SortOrder = sortOrder;

            var dsKhoaHoc = await query.ToListAsync();
            ViewBag.TotalCount = dsKhoaHoc.Count;
            return View(dsKhoaHoc);
        }

        // GET: KhoaHoc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.LopHocs!)
                    .ThenInclude(l => l.DangKyHocs)
                .FirstOrDefaultAsync(m => m.MaKhoaHoc == id);

            if (khoaHoc == null) return NotFound();

            return View(khoaHoc);
        }

        // GET: KhoaHoc/Create (Admin)
        [AuthorizeRole("Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: KhoaHoc/Create (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create([Bind("MaKhoaHoc,TenKhoaHoc,NgoaiNgu,CapDo,HocPhi,SoBuoi,MoTa,TrangThai")] KhoaHoc khoaHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(khoaHoc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Thêm mới khóa học '{khoaHoc.TenKhoaHoc}' thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(khoaHoc);
        }

        // GET: KhoaHoc/Edit/5 (Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var khoaHoc = await _context.KhoaHocs.FindAsync(id);
            if (khoaHoc == null) return NotFound();

            return View(khoaHoc);
        }

        // POST: KhoaHoc/Edit/5 (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MaKhoaHoc,TenKhoaHoc,NgoaiNgu,CapDo,HocPhi,SoBuoi,MoTa,TrangThai")] KhoaHoc khoaHoc)
        {
            if (id != khoaHoc.MaKhoaHoc) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khoaHoc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật khóa học '{khoaHoc.TenKhoaHoc}' thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhoaHocExists(khoaHoc.MaKhoaHoc)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khoaHoc);
        }

        // GET: KhoaHoc/Delete/5 (Admin)
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.LopHocs)
                .FirstOrDefaultAsync(m => m.MaKhoaHoc == id);

            if (khoaHoc == null) return NotFound();

            return View(khoaHoc);
        }

        // POST: KhoaHoc/Delete/5 (Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khoaHoc = await _context.KhoaHocs
                .Include(k => k.LopHocs)
                .FirstOrDefaultAsync(k => k.MaKhoaHoc == id);

            if (khoaHoc != null)
            {
                if (khoaHoc.LopHocs != null && khoaHoc.LopHocs.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa khóa học '{khoaHoc.TenKhoaHoc}' vì đang có {khoaHoc.LopHocs.Count} lớp học liên kết.";
                    return RedirectToAction(nameof(Index));
                }

                _context.KhoaHocs.Remove(khoaHoc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa khóa học '{khoaHoc.TenKhoaHoc}' thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool KhoaHocExists(int id)
        {
            return _context.KhoaHocs.Any(e => e.MaKhoaHoc == id);
        }
    }
}
