using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IHttpClientFactory _httpClientFactory;        
        private readonly IBaseService _baseService;
        private string? _url;
        public VillaNumberService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IBaseService baseService) 
        {
            _httpClientFactory = httpClientFactory;
            _url = configuration.GetValue<string>("ServiceURL:VillaAPI");
            _baseService = baseService;
        }

        public async Task<T> CreateAsync<T>(VillaNumberCreateDTO villaNumberCreateDTO)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = villaNumberCreateDTO,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/"
            });
        }

        public async Task<T> DeleteAsync<T>(int villaNo)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.DELETE,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/{villaNo}"
            });
        }

        public async Task<T> GetAllAsync<T>()
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/"
            });
        }

        public async Task<T> GetAsync<T>(int villaNo)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/{villaNo}"
            });
        }

        public async Task<T> UpdateAsync<T>(VillaNumberUpdateDTO villaNumberUpdateDTO)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.PUT,
                Data = villaNumberUpdateDTO,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaNumberAPI/{villaNumberUpdateDTO.VillaNo}"
            });
        }
    }
}
