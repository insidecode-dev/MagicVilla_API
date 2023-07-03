using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<LogInResponseDTO> LogIn(LogInRequestDto logInRequestDTO);
        Task<LocalUser> Register(RegistrationRequestDTO logInRequestDTO);
    }
}
