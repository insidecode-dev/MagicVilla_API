using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MagicVilla_VillaAPI.Filters
{
    public class CustomExceptionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is FileNotFoundException file)
            {
                context.Result = new ObjectResult("file not found but handled in filter")
                {
                    StatusCode = 503,
                };

                // this flag makes the setting disable that we added in program.cs for exception handler, if this exception is catched context inside if statement will be returned, not setting made in program.cs
                context.ExceptionHandled = true;    
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
           
        }
    }
}
