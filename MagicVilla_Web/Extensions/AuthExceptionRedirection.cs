using MagicVilla_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MagicVilla_Web.Extensions
{
    public class AuthExceptionRedirection : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            // in case if AuthException exception catched, web page will be directed to log in page
            if (context.Exception is AuthException)
             context.Result = new RedirectToActionResult("Login","Auth", null); 
        }
    }
}
