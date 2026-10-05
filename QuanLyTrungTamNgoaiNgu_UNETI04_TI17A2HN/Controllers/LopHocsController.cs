using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
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
        public async Task<IActionResult> Index(string searchKeyword, int? maKhoaHoc, string ngoaiNgu, string trangThai, decimal? giaTu, decimal? giaDen, string sortOrder, int page = 1)
        {
            int pageSize = 5;
            var query = _context.LopHocs.Include(l => l.KhoaHoc).AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(l => l.TenLop.Contains(searchKeyword));
            }
            if (maKhoaHoc.HasValue)
            {
                query = query.Where(l => l.MaKhoaHoc == maKhoaHoc);
            }
            if (!string.IsNullOrEmpty(ngoaiNgu))
            {
                query = query.Where(l => l.KhoaHoc.NgoaiNgu.Contains(ngoaiNgu));
            }
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(l => l.TrangThai == trangThai);
            }
            if (giaTu.HasValue)
            {
                query = query.Where(l => l.HocPhi >= giaTu);
            }
            if (giaDen.HasValue)
            {
                query = query.Where(l => l.HocPhi <= giaDen);
            }

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
                default:
                    query = query.OrderBy(l => l.TenLop);
                    break;
            }

            int totalItems = await query.CountAsync();
            var danhSach = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new LopHocIndexViewModel
            {
                DanhSachLop = danhSach,
                SearchKeyword = searchKeyword,
                MaKhoaHoc = maKhoaHoc,
                NgoaiNgu = ngoaiNgu,
                TrangThai = trangThai,
                GiaTu = giaTu,
                GiaDen = giaDen,
                SortOrder = sortOrder,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            ViewBag.KhoaHocList = new SelectList(await _context.KhoaHocs.ToListAsync(), "MaKhoaHoc", "TenKhoaHoc", maKhoaHoc);
            ViewBag.NgoaiNguList = new SelectList(await _context.KhoaHocs.Select(k => k.NgoaiNgu).Distinct().ToListAsync(), ngoaiNgu);
            ViewBag.TrangThaiList = new SelectList(new List<string> { "Sắp khai giảng", "Đang học", "Đã kết thúc" }, trangThai);

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
                .FirstOrDefaultAsync(m => m.MaLop == id);
            if (lopHoc == null)
            {
                return NotFound();
            }

            return View(lopHoc);
        }

        // GET: LopHocs/Create
        public IActionResult Create()
        {
            ViewData["MaKhoaHoc"] = new SelectList(_context.Set<KhoaHoc>(), "MaKhoaHoc", "CapDo");
            return View();
        }

        // POST: LopHocs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLop,TenLop,MaKhoaHoc,NgayKhaiGiang,LichHoc,PhongHoc,SoLuongToiDa,HocPhi,TrangThai")] LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lopHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaKhoaHoc"] = new SelectList(_context.Set<KhoaHoc>(), "MaKhoaHoc", "CapDo", lopHoc.MaKhoaHoc);
            return View(lopHoc);
        }

        // GET: LopHocs/Edit/5
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
            ViewData["MaKhoaHoc"] = new SelectList(_context.Set<KhoaHoc>(), "MaKhoaHoc", "CapDo", lopHoc.MaKhoaHoc);
            return View(lopHoc);
        }

        // POST: LopHocs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            ViewData["MaKhoaHoc"] = new SelectList(_context.Set<KhoaHoc>(), "MaKhoaHoc", "CapDo", lopHoc.MaKhoaHoc);
            return View(lopHoc);
        }

        // GET: LopHocs/Delete/5
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

        // POST: LopHocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lopHoc = await _context.LopHocs.FindAsync(id);
            if (lopHoc != null)
            {
                _context.LopHocs.Remove(lopHoc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LopHocExists(int id)
        {
            return _context.LopHocs.Any(e => e.MaLop == id);
        }
    }
}
