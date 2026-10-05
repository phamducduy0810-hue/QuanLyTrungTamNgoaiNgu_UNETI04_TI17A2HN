// Họ và tên: Phạm Văn Công
// Mã sinh viên: 23103100115
// Nội dung thực hiện: Module 1 - ActionFilter Phân quyền truy cập dựa trên Session (Admin / HocVien)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute 
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var maTaiKhoan = session.GetInt32("MaTaiKhoan");
            var vaiTro = session.GetString("VaiTro");

            if (!maTaiKhoan.HasValue || string.IsNullOrEmpty(vaiTro))
            {
                context.Result = new RedirectToActionResult("Login", "TaiKhoan", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(vaiTro))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "TaiKhoan", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
