using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace MagicVilla_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            LogInRequestDto? logInRequestDTO = new ();
            return View(logInRequestDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(LogInRequestDto logInRequestDTO)
        {
            ApiResponse apiResponse = await _authService.LogInAsync<ApiResponse>(logInRequestDTO);
            if (apiResponse!=null && apiResponse.IsSuccess)
            {
                LogInResponseDTO logInResponseDTO = JsonConvert.DeserializeObject<LogInResponseDTO>(Convert.ToString(apiResponse.Result));

                // 

                // without the lines below even if the user is logged in, for every calling method he'll face with login page
                //we've set the session and recieved the token but still we have not told the HTTP context on this web application that this user has signed in . If the HTTP context is not aware of that, then it always think that the user has not signed in  
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, logInResponseDTO.LocalUser.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, logInResponseDTO.LocalUser.Role));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme ,principal);

                //

                HttpContext.Session.SetString(key:StaticDetails.SessionToken,value:logInResponseDTO.Token);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("ErrorMessages",apiResponse.ErrorMessages.FirstOrDefault());
                return View(logInRequestDTO);
            }            
        }


        [HttpGet]
        public IActionResult Register()
        {            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            ApiResponse? apiResponse = await _authService.RegisterAsync<ApiResponse>(registrationRequestDTO);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                return RedirectToAction("LogIn");
            }
            return View();
        }


        public async Task<IActionResult> LogOut()
        {   
            await HttpContext.SignOutAsync();
            HttpContext.Session.SetString(key: StaticDetails.SessionToken, value: "");
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
