﻿using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ApiResponse _apiResponse;
        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _apiResponse = new();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogInRequestDto logInRequestDto)
        {
            var logInResponse = await _userRepository.LogIn(logInRequestDto);
            if (logInResponse.LocalUser==null || string.IsNullOrEmpty(logInResponse.Token))
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.ErrorMessages.Add("username or password is incorrect");
                _apiResponse.IsSuccess = false;
                return BadRequest(_apiResponse);
            }

            _apiResponse.IsSuccess=true;
            _apiResponse.Result = logInResponse;
            _apiResponse.StatusCode=HttpStatusCode.OK;
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
            _apiResponse.IsSuccess=true;                
            _apiResponse.StatusCode=HttpStatusCode.OK; 
            return Ok(_apiResponse);
        }
    }
}