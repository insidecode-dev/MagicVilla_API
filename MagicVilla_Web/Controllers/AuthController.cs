using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
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


                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(logInResponseDTO.Token);

                // 

                // without the lines below even if the user is logged in, for every calling method he'll face with login page
                //we've set the session and recieved the token but still we have not told the HTTP context on this web application that this user has signed in . If the HTTP context is not aware of that, then it always think that the user has not signed in  
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(x => x.Type == "unique_name").Value));
                identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(x=>x.Type=="role").Value)) ;
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
            // we initiallize roles from code we write in visual studio, and these roles are executed in memory, I mean it does not come from database, as a result, if such role does not exist, it will be created (Register method of UserRespository in api)
            var roleList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = StaticDetails.Admin,
                    Value = StaticDetails.Admin
                },
                new SelectListItem
                {
                    Text = StaticDetails.Customer,
                    Value = StaticDetails.Customer
                }
            };
            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            if (string.IsNullOrEmpty(registrationRequestDTO.Role))
            {
                registrationRequestDTO.Role = StaticDetails.Customer;
            }
            ApiResponse? apiResponse = await _authService.RegisterAsync<ApiResponse>(registrationRequestDTO);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                return RedirectToAction("LogIn");
            }
            var roleList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = StaticDetails.Admin,
                    Value = StaticDetails.Admin
                },
                new SelectListItem
                {
                    Text = StaticDetails.Customer,
                    Value = StaticDetails.Customer
                }
            };
            ViewBag.RoleList = roleList;
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
