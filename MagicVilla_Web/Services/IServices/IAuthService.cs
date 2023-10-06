using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LogInAsync<T>(LogInRequestDto? logInRequestDto);
        Task<T> RegisterAsync<T>(RegistrationRequestDTO? userToCreate);
        Task<T> LogOutAsync<T>(TokenDTO tokenDTO);
    }
}