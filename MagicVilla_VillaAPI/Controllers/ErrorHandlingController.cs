using Microsoft.AspNetCore.Authorization;
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
        public IActionResult ProcessError() => Problem();        
    }
}
