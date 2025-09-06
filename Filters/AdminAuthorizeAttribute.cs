using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShipmentTrackingSystem.Filters
{
    // فلتر بسيط لتأمين صفحات الأدمن بالجلسة
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var isAdmin = context.HttpContext.Session.GetString("isAdmin") == "1";
            if (!isAdmin)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectResult($"/Admin/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
        }
    }
}
