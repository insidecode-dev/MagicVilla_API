using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    // the attribute below makes it there for all versions, no matter what the version is   
    [ApiVersionNeutral]
    public class UsersAuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ApiResponse _apiResponse;
        public UsersAuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _apiResponse = new();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogInRequestDto logInRequestDto)
        {
            var tokenDto = await _userRepository.LogIn(logInRequestDto);
            if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.ErrorMessages.Add("username or password is incorrect");
                _apiResponse.IsSuccess = false;
                return BadRequest(_apiResponse);
            }

            _apiResponse.IsSuccess = true;
            _apiResponse.Result = tokenDto;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var ifUserNameUnique = _userRepository.IsUniqueUser(registrationRequestDTO.UserName);
            if (!ifUserNameUnique)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add("username already exists");
                return BadRequest(_apiResponse);
            }

            var user = await _userRepository.Register(registrationRequestDTO);
            if (user == null)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add("error while registration");
                return BadRequest(_apiResponse);
            }
            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.Result = user;
            return Ok(_apiResponse);
        }
    }
}
