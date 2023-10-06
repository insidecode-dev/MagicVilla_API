using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseService _baseService;
        private string? _url;
        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IBaseService baseService)
        {
            _httpClientFactory = httpClientFactory;
            _url = configuration.GetValue<string>("ServiceURL:VillaAPI");
            _baseService = baseService;
        }

        public async Task<T> LogInAsync<T>(LogInRequestDto logInRequestDto)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = logInRequestDto,
                ApiUrl = _url + "api/UsersAuth/login"
            }, withBearer:false);
        }

        public async Task<T> LogOutAsync<T>(TokenDTO tokenDTO)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,                
                Data = tokenDTO,   
                ApiUrl = _url + "api/UsersAuth/revoke"
            });
        }

        public async Task<T> RegisterAsync<T>(RegistrationRequestDTO userToCreate)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = userToCreate,
                ApiUrl = _url + "api/UsersAuth/register"
            }, withBearer:false);
        }
    }
}
