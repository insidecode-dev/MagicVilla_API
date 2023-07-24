using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class VillaNumberService : BaseService, IVillaNumberService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string? _url;
        public VillaNumberService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _url = configuration.GetValue<string>("ServiceURL:VillaAPI");
        }

        public Task<T> CreateAsync<T>(VillaNumberCreateDTO villaNumberCreateDTO, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = villaNumberCreateDTO,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/",
                Token = token
            });
        }

        public Task<T> DeleteAsync<T>(int villaNo, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.DELETE,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/{villaNo}",
                Token = token
            });
        }

        public Task<T> GetAllAsync<T>(string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/",
                Token = token
            });
        }

        public Task<T> GetAsync<T>(int villaNo, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/{villaNo}",
                Token = token
            });
        }

        public Task<T> UpdateAsync<T>(VillaNumberUpdateDTO villaNumberUpdateDTO, string token)
        {
            return SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.PUT,
                Data = villaNumberUpdateDTO,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/{villaNumberUpdateDTO.VillaNo}",
                Token = token
            });
        }
    }
}
