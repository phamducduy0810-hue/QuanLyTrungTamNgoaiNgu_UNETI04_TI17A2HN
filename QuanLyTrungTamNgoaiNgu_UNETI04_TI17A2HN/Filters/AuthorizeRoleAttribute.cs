// Họ và tên: Phạm Văn Công
// Mã sinh viên: 23103100115
// Nội dung thực hiện: Module 1 - ActionFilter Phân quyền truy cập dựa trên Session (Admin / HocVien)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace QuanLyTrungTamNgoaiNgu_UNETI04_TI17A2HN.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");

            // Chưa đăng nhập -> về trang Login, nhớ trang đang truy cập
            if (string.IsNullOrEmpty(vaiTro))
            {
                var request = context.HttpContext.Request;
                var returnUrl = request.Path + request.QueryString;
                context.Result = new RedirectToActionResult("Login", "TaiKhoan", new { returnUrl });
                return;
            }

            // Đã đăng nhập nhưng sai vai trò -> trang từ chối truy cập
            if (_roles.Length > 0 && !_roles.Contains(vaiTro))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "TaiKhoan", null);
            }
        }
    }
}
