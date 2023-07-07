using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string? _url;
        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _url = configuration.GetValue<string>("ServiceURL:VillaAPI");
        }

        public Task<T> LogInAsync<T>(LogInRequestDto logInRequestDto)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = logInRequestDto,
                ApiUrl = _url + "api/UsersAuth/login"
            });
        }

        public Task<T> RegisterAsync<T>(RegistrationRequestDTO userToCreate)
        {            
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = userToCreate,
                ApiUrl = _url + "api/UsersAuth/register"
            });
        }
    }
}
