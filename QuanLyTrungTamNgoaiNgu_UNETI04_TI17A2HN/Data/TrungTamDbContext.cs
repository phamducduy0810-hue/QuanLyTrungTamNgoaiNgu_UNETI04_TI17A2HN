// Họ và tên: Phạm Văn Công
// Mã sinh viên: 23103100115
// Nội dung thực hiện: TrungTamDbContext tương thích dự án

using Microsoft.EntityFrameworkCore;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Data
{
    public class TrungTamDbContext : ApplicationDbContext
    {
        public TrungTamDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public TrungTamDbContext(DbContextOptions<TrungTamDbContext> options) : base(new DbContextOptionsBuilder<ApplicationDbContext>().Options)
        {
        }
    }
}
