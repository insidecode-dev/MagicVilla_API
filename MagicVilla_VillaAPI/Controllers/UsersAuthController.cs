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

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDtoResponse = await _userRepository.RefreshAccessToken(tokenDTO);

                if (tokenDtoResponse == null || string.IsNullOrEmpty(tokenDtoResponse.AccessToken))
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Token invalid");
                    return BadRequest(_apiResponse);
                }

                _apiResponse.IsSuccess = true;
                _apiResponse.Result = tokenDtoResponse;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            else
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Result = "Invalid input";
                return BadRequest(_apiResponse);
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                await _userRepository.RevokefreshToken(tokenDTO);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                return Ok(_apiResponse);
            }

            _apiResponse.StatusCode = HttpStatusCode.BadRequest;
            _apiResponse.IsSuccess = false;
            _apiResponse.Result = "Invalid input";
            return BadRequest(_apiResponse);
        }
    }
}
