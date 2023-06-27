using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace MagicVilla_Web.Services
{
    public class BaseService : IBaseService
    {
        public ApiResponse responseModel { get; set; }
        public IHttpClientFactory _httpClientFactory { get; set; }
        public BaseService(IHttpClientFactory httpClientFactory)
        {
            responseModel = new();
            _httpClientFactory = httpClientFactory;
        }
        public async Task<T> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                //The function begins by creating an instance of HttpClient using the _httpClientFactory.CreateClient method. It retrieves a named client called "MagicVilla."
                var client = _httpClientFactory.CreateClient("MagicVilla");

                //It creates an instance of HttpRequestMessage to represent the HTTP request and sets the "Accept" header to indicate that the expected response type is JSON.
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");

                //message.Headers.Add("Content-Type", "application/json");
                //It sets the request URI of the HttpRequestMessage based on the ApiRequest.ApiUrl property.
                message.RequestUri = new(apiRequest.ApiUrl);

                if (apiRequest.Data != null)
                {
                    //If the ApiRequest.Data property is not null, it serializes the data object to JSON using JsonConvert.SerializeObject and sets it as the content of the HttpRequestMessage using StringContent.
                    message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data), Encoding.UTF8, "application/json");
                }

                //Based on the ApiRequest.ApiType property, it sets the HttpMethod property of the HttpRequestMessage to the appropriate HTTP method (POST, PUT, DELETE, or GET).
                switch (apiRequest.ApiType)
                {
                    case StaticDetails.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case StaticDetails.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case StaticDetails.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                //It initializes an HttpResponseMessage object called apiResponse and sends the request asynchronously using the SendAsync method of the HttpClient. The response is awaited to ensure the function waits for the API response.
                HttpResponseMessage apiRespone = null;
                apiRespone = await client.SendAsync(message);

                //It reads the content of the API response as a string using ReadAsStringAsync.
                var apiContent = await apiRespone.Content.ReadAsStringAsync();

                //It deserializes the API response content string to an object of type T using JsonConvert.DeserializeObject<T> and assigns it to the APIResponse variable.
                var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
                return APIResponse;
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
    }
}

//Deserialization is the opposite of serialization. It involves converting data from a serialized format (e.g., JSON) back into its original object representation. In this case, JsonConvert.DeserializeObject<T> is used to perform the deserialization. It takes the JSON string as input and returns an object of type T, which matches the specified generic type parameter.