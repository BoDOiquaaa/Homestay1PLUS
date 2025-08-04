using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Homestay1.Filters
{
    /// <summary>
    /// Filter kiểm tra quyền truy cập - chỉ Owner (RoleID = 2) mới được phép
    /// </summary>
    public class OwnerAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            // Kiểm tra có UserID trong session không
            var userID = session.GetInt32("UserID");
            var roleID = session.GetInt32("RoleID");

            // Debug log
            System.Diagnostics.Debug.WriteLine($"=== OWNER AUTHORIZATION CHECK ===");
            System.Diagnostics.Debug.WriteLine($"UserID: {userID}");
            System.Diagnostics.Debug.WriteLine($"RoleID: {roleID}");
            System.Diagnostics.Debug.WriteLine($"Controller: {context.Controller.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Action: {context.ActionDescriptor.DisplayName}");

            if (!userID.HasValue || !roleID.HasValue)
            {
                System.Diagnostics.Debug.WriteLine("❌ No session found - redirecting to login");

                // Chưa đăng nhập -> redirect về Login
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "area", "ad" },
                    { "controller", "Account" },
                    { "action", "Login" }
                });
                return;
            }

            // Kiểm tra role có phải Owner (RoleID = 2) không
            if (roleID.Value != 2)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Access denied - RoleID {roleID.Value} is not Owner");

                // Không phải Owner -> trả về Access Denied
                context.Result = new ViewResult
                {
                    ViewName = "~/Areas/ad/Views/Shared/AccessDenied.cshtml"
                };
                return;
            }

            System.Diagnostics.Debug.WriteLine("✅ Authorization passed - User is Owner");
            base.OnActionExecuting(context);
        }
    }
}