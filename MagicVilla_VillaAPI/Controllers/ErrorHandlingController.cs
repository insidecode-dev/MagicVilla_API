using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("ErrorHandling")]
    [ApiController]
    [AllowAnonymous]
    [ApiVersionNeutral]
    [ApiExplorerSettings(IgnoreApi =true)] // this makes this endpoint not exist in swagger UI, this attribute ignores it 
    public class ErrorHandlingController : ControllerBase
    {
        [Route("ProcessError")]
        public IActionResult ProcessError([FromServices] IHostEnvironment hostEnvironment)
        {
            if (hostEnvironment.IsDevelopment())
            {
                // custom logic
                var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
                return Problem(
                    detail : feature.Error.InnerException is not null? feature.Error.InnerException.StackTrace :feature.Error.StackTrace,
                    title : feature.Error.Message,
                    instance : hostEnvironment.EnvironmentName
                    );
            }

            else
            {
                return Problem();
            }
        }
    }
}
