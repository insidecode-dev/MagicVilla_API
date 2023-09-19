using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices
{
    public interface ITokenProvider
    {
        // when we work with token, we want to do three things : 
        // 1. Setting token when user logs in 
        void SetToken(TokenDTO tokenDTO);
        // 2. retrieving token when we're making api calls 
        TokenDTO? GetToken();
        // 3. clear or reset token when user logs out
        void ClearToken();
    }
}
