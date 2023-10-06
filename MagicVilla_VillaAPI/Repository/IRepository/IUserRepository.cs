using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<TokenDTO> LogIn(LogInRequestDto logInRequestDTO);
        Task<UserDTO> Register(RegistrationRequestDTO logInRequestDTO);
        Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO);
        Task RevokefreshToken(TokenDTO tokenDTO);
    }
}
