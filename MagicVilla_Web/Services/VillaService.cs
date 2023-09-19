using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using static MagicVilla_Utility.StaticDetails;

namespace MagicVilla_Web.Services
{
    public class VillaService : IVillaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseService _baseService;
        private string? _url;
        public VillaService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IBaseService baseService) 
        {
            _httpClientFactory = httpClientFactory;
            _url = configuration.GetValue<string>("ServiceURL:VillaAPI");
            _baseService = baseService;
        }

        public async Task<T> CreateAsync<T>(VillaCreateDTO villaCreateDTO)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.POST,
                Data = villaCreateDTO,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaAPI/NewVilla",
                ContentType = ContentType.MultipartFormData
            });
        }

        public async Task<T> DeleteAsync<T>(int id)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.DELETE,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaAPI/{id}"
            });
        }

        public async Task<T> GetAllAsync<T>()
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaAPI/"
            });
        }

        public async Task<T> GetAsync<T>(int id)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.GET,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaAPI/{id}"
            });
        }

        public async Task<T> UpdateAsync<T>(VillaUpdateDTO villaUpdateDTO)
        {
            return await _baseService.SendAsync<T>(new ApiRequest()
            {
                ApiType = MagicVilla_Utility.StaticDetails.ApiType.PUT,
                Data = villaUpdateDTO,
                ApiUrl = _url + $"api/{StaticDetails.CurrentAPIVersion}/VillaAPI/{villaUpdateDTO.Id}",
                ContentType = ContentType.MultipartFormData
            });
        }
    }
}
