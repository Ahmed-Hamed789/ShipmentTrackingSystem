using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace ShipmentTrackingSystem.Filters
{
    public class TrackAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            if (ctx.HttpContext.Session.GetInt32("userId") == null)
            {
                var returnUrl = ctx.HttpContext.Request.Path + ctx.HttpContext.Request.QueryString;
                ctx.Result = new RedirectToActionResult("Login", "Users", new { returnUrl });
            }
        }
    }
}
