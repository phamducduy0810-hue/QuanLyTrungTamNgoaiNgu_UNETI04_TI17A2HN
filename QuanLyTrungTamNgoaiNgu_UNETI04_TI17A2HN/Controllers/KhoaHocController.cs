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
        private readonly TrungTamDbContext _context;

        public KhoaHocController(TrungTamDbContext context)
        {
            _context = context;
        }

        // GET: KhoaHoc
        public async Task<IActionResult> Index(string? searchNgoaiNgu, string? searchCapDo, string? searchKeyword)
        {
            IQueryable<KhoaHoc> query = _context.KhoaHocs.Include(k => k.LopHocs);

            if (!string.IsNullOrWhiteSpace(searchNgoaiNgu))
            {
                query = query.Where(k => k.NgoaiNgu == searchNgoaiNgu);
            }

            if (!string.IsNullOrWhiteSpace(searchCapDo))
            {
                query = query.Where(k => k.CapDo == searchCapDo); 
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string kw = searchKeyword.Trim().ToLower();
                query = query.Where(k => k.TenKhoaHoc.ToLower().Contains(kw) || k.MoTa.ToLower().Contains(kw));
            }

            ViewBag.SearchNgoaiNgu = searchNgoaiNgu;
            ViewBag.SearchCapDo = searchCapDo;
            ViewBag.SearchKeyword = searchKeyword;

            var dsKhoaHoc = await query.OrderByDescending(k => k.MaKhoaHoc).ToListAsync();
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
        public async Task<IActionResult> Create([Bind("MaKhoaHoc,TenKhoaHoc,NgoaiNgu,CapDo,HocPhi,MoTa")] KhoaHoc khoaHoc)
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
        public async Task<IActionResult> Edit(int id, [Bind("MaKhoaHoc,TenKhoaHoc,NgoaiNgu,CapDo,HocPhi,MoTa")] KhoaHoc khoaHoc)
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
