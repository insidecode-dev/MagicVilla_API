using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class VillaService : BaseService, IVillaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string? _url;
        public VillaService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _url = configuration.GetValue<string>("ServiceURL:VillaAPI");
        }

        public Task<T> CreateAsync<T>(VillaCreateDTO villaCreateDTO, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = villaCreateDTO,
                ApiUrl = _url + "api/VillaAPI/NewVilla",
                Token = token
            });
        }

        public Task<T> DeleteAsync<T>(int id, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.DELETE,
                ApiUrl = _url + $"api/VillaAPI/{id}",
                Token = token
            });
        }

        public Task<T> GetAllAsync<T>(string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + "api/VillaAPI/",
                Token = token
            });
        }

        public Task<T> GetAsync<T>(int id, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + $"api/VillaAPI/{id}",
                Token = token
            });
        }

        public Task<T> UpdateAsync<T>(VillaUpdateDTO villaUpdateDTO, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.PUT,
                Data = villaUpdateDTO,
                ApiUrl = _url + $"api/VillaAPI/{villaUpdateDTO.Id}",
                Token = token
            });
        }
    }
}
