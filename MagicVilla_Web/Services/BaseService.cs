using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using static MagicVilla_Utility.StaticDetails;

namespace MagicVilla_Web.Services
{
    public class BaseService : IBaseService
    {
        public ApiResponse responseModel { get; set; }
        private IHttpClientFactory _httpClientFactory { get; set; }
        private readonly ITokenProvider _tokenProvider;
        protected readonly string VillaApiUrl;
        private readonly IHttpContextAccessor _httpContextAccessor; // for user singing out
        private readonly IApiMessageRequestBuilder _apiMessageRequestBuilder;
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IApiMessageRequestBuilder apiMessageRequestBuilder)
        {
            responseModel = new();
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
            VillaApiUrl = configuration.GetValue<string>("ServiceURL:VillaAPI");
            _httpContextAccessor = httpContextAccessor;
            _apiMessageRequestBuilder = apiMessageRequestBuilder;
        }
        public async Task<T> SendAsync<T>(ApiRequest apiRequest, bool withBearer = true)
        {
            try
            {
                //The function begins by creating an instance of HttpClient using the _httpClientFactory.CreateClient method. It retrieves a named client called "MagicVilla."
                var client = _httpClientFactory.CreateClient("MagicVilla");

                // separated request message to another file 
                var messageFactory = () =>
                {
                    return _apiMessageRequestBuilder.Build(apiRequest);
                };

                // It initializes an HttpResponseMessage object called apiResponse and sends the request asynchronously using the SendAsync method of the HttpClient. The response is awaited to ensure the function waits for the API response.
                HttpResponseMessage? httpResponseMessage = null;


                httpResponseMessage = await SendWithRefreshTokenAsync(client, messageFactory, withBearer );

                ApiResponse finalApiResponse = new()
                {
                    IsSuccess = false,
                };

                // It reads the content of the API response as a string using ReadAsStringAsync.
                //var apiContent = await httpResponseMessage.Content.ReadAsStringAsync();


                try
                {
                    switch (httpResponseMessage.StatusCode)
                    {                        
                        case HttpStatusCode.Unauthorized:
                            finalApiResponse.ErrorMessages = new List<string> { "Unauthorized" };
                            break;                        
                        case HttpStatusCode.Forbidden:
                            finalApiResponse.ErrorMessages = new List<string> { "Access Denied" };
                            break;
                        case HttpStatusCode.NotFound:
                            finalApiResponse.ErrorMessages = new List<string> { "Not Found" };
                            break;                        
                            break;
                        case HttpStatusCode.InternalServerError:
                            finalApiResponse.ErrorMessages = new List<string> { "Internal Server Error" };
                            break;                        
                        default:
                            var apiContent  = await httpResponseMessage.Content.ReadAsStringAsync();
                            finalApiResponse.IsSuccess = true;
                            // in code line below we initialize ErrorMessages property of ApiResponse object with the string value of apiContent
                            finalApiResponse = JsonConvert.DeserializeObject<ApiResponse>(apiContent);
                            break;
                    }
                }               

                catch (Exception e)
                {
                    finalApiResponse.ErrorMessages = new List<string> { "Error Encountered", e.Message.ToString() };                    
                }
                var res = JsonConvert.SerializeObject(finalApiResponse);
                var returnObj = JsonConvert.DeserializeObject<T>(res);
                return returnObj;
            }

            // if refresh token expired it will be directly logged out and redirected to log in page 
            catch (AuthException)
            {
                throw;
            }

            //If an exception occurs during the process (caught by the catch block), it creates an instance of ApiResponse and populates it with the error message from the exception. The ApiResponse is then serialized to JSON and deserialized to an object of type T (same as step 8) before being returned as the error response.
            catch (Exception ex)
            {
                var dto = new ApiResponse
                {
                    ErrorMessages = new List<string> { ex.Message.ToString() },
                    IsSuccess = false
                };

                var res = JsonConvert.SerializeObject(dto);
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;
            }


        }

        private async Task<HttpResponseMessage> SendWithRefreshTokenAsync(HttpClient httpClient, Func<HttpRequestMessage> httpRequestMessageFactory, bool withBearer = true)
        {
            if (!withBearer)
            {
                return await httpClient.SendAsync(httpRequestMessageFactory());
            }
            else
            {
                TokenDTO tokenDTO = _tokenProvider.GetToken();
                if (tokenDTO!=null && !string.IsNullOrEmpty(tokenDTO.AccessToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDTO.AccessToken);  
                }

                try
                {
                    var response = await httpClient.SendAsync(httpRequestMessageFactory());
                    if (response.IsSuccessStatusCode) return response;

                    // if this fails then we can pass refresh token 
                    if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // generate new token with refresh token / sign in with the new token and them try
                        await InvokeRefreshTokenEndpointAsync(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);

                        response = await httpClient.SendAsync(httpRequestMessageFactory());
                        return response;
                    }
                    return response;
                }

                // if refresh token expired it will be directly logged out and redirected to log in page 
                catch (AuthException)
                {
                    throw;
                }      
                
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // refresh token and retry the request
                        await InvokeRefreshTokenEndpointAsync(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                        return await httpClient.SendAsync(httpRequestMessageFactory());
                    }
                    throw ;
                }
            }
            
        }

        private async Task InvokeRefreshTokenEndpointAsync(HttpClient httpClient, string existingAccessToken, string existingRefreshToken)
        {
            HttpRequestMessage httpRequestMessage = new();
            httpRequestMessage.Headers.Add("Accept", "application/json");
            httpRequestMessage.RequestUri = new Uri($"{VillaApiUrl}api/UsersAuth/refresh"); // {StaticDetails.CurrentAPIVersion}/
            httpRequestMessage.Method = HttpMethod.Post;    
            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(new TokenDTO()
            {
                AccessToken = existingAccessToken,
                RefreshToken = existingRefreshToken
            }), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(httpRequestMessage);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

            if (apiResponse?.IsSuccess!=true)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync();
                _tokenProvider.ClearToken();
                throw new AuthException();
            }
            else
            {
                var tokenDataStr = JsonConvert.SerializeObject(apiResponse.Result);
                var tokenDto = JsonConvert.DeserializeObject<TokenDTO>(tokenDataStr);

                if (tokenDto!=null && !string.IsNullOrEmpty(tokenDto.AccessToken))
                {
                    // new method to sign in with the new token that we have 
                    await SignInWithNewTokens(tokenDto);

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDto.AccessToken);
                }
            }
        }

        private async Task SignInWithNewTokens(TokenDTO tokenDTO)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenDTO.AccessToken);

            // 

            // without the lines below even if the user is logged in, for every calling method he'll face with login page
            //we've set the session and recieved the token but still we have not told the HTTP context on this web application that this user has signed in . If the HTTP context is not aware of that, then it always think that the user has not signed in  
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(x => x.Type == "unique_name").Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(x => x.Type == "role").Value));
            var principal = new ClaimsPrincipal(identity);
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            //

            _tokenProvider.SetToken(tokenDTO);
        }
    }
}

//Deserialization is the opposite of serialization. It involves converting data from a serialized format (e.g., JSON) back into its original object representation. In this case, JsonConvert.DeserializeObject<T> is used to perform the deserialization. It takes the JSON string as input and returns an object of type T, which matches the specified generic type parameter.