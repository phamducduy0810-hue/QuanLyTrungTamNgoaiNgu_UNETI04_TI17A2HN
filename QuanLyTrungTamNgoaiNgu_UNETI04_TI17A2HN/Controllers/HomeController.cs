using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data;
using QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Models;
using System.Diagnostics;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalKhoaHoc = await _context.KhoaHocs.CountAsync();
            ViewBag.TotalLopHoc = await _context.LopHocs.CountAsync();
            ViewBag.TotalHocVien = await _context.HocViens.CountAsync();

            var noiBat = await _context.KhoaHocs
                .Where(k => k.TrangThai)
                .OrderBy(k => k.MaKhoaHoc)
                .Take(4)
                .ToListAsync();

            return View(noiBat);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
