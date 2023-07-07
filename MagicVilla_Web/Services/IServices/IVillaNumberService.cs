using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices
{
    public interface IVillaNumberService
    {
        Task<T> GetAllAsync<T>(string token);
        Task<T> GetAsync<T>(int id, string token);
        Task<T> CreateAsync<T>(VillaNumberCreateDTO villaNumberCreateDTO, string token);
        Task<T> UpdateAsync<T>(VillaNumberUpdateDTO villaNumberUpdateDTO, string token);
        Task<T> DeleteAsync<T>(int id, string token);

    }
}
