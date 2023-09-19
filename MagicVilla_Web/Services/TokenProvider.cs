using MagicVilla_Utility;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class TokenProvider : ITokenProvider
    {
        // when we have to work with cookies or session, we have to inject the httpcontextprovider
        private readonly IHttpContextAccessor _contextAccessor;
        public TokenProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(StaticDetails.AccessToken);
        }

        public TokenDTO? GetToken()
        {
            try
            {
                bool hasAccessToken = _contextAccessor.HttpContext.Request.Cookies.TryGetValue(StaticDetails.AccessToken, out string accessToken);
                return hasAccessToken ? new TokenDTO { AccessToken = accessToken } : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void SetToken(TokenDTO tokenDTO)
        {
            var cookieOptions = new CookieOptions { Expires = DateTime.UtcNow.AddDays(60) };
            _contextAccessor.HttpContext.Response.Cookies.Append(StaticDetails.AccessToken, tokenDTO.AccessToken, cookieOptions);
        }
    }
}
