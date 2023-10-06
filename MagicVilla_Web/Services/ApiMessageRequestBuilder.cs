using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using static MagicVilla_Utility.StaticDetails;
using Newtonsoft.Json;
using System.Text;
using MagicVilla_Utility;

namespace MagicVilla_Web.Services
{
    public class ApiMessageRequestBuilder : IApiMessageRequestBuilder
    {
        public HttpRequestMessage Build(ApiRequest apiRequest)
        {
            //It creates an instance of HttpRequestMessage to represent the HTTP request and sets the "Accept" header to indicate that the expected response type is JSON.
            HttpRequestMessage message = new();

            if (apiRequest.ContentType == ContentType.Json)
            {
                message.Headers.Add("Accept", "application/json");
            }
            else if (apiRequest.ContentType == ContentType.MultipartFormData)
            {
                message.Headers.Add("Accept", "*/*"); // this is for multipart form data
            }

            //message.Headers.Add("Content-Type", "application/json");
            //It sets the request URI of the HttpRequestMessage based on the ApiRequest.ApiUrl property.
            message.RequestUri = new(apiRequest.ApiUrl);

            //validating token before sending request
            //if (withBearer && _tokenProvider.GetToken() != null)
            //{
            //    var token = _tokenProvider.GetToken();
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            //}

            // we do this more dynamic because content is not just json string, it also consists of image
            if (apiRequest.ContentType == ContentType.MultipartFormData)
            {
                var content = new MultipartFormDataContent();

                foreach (var property in apiRequest.Data.GetType().GetProperties())
                {
                    var value = property.GetValue(apiRequest.Data);
                    if (value is FormFile)
                    {
                        var file = (FormFile)value;
                        if (file is not null)
                        {
                            content.Add(new StreamContent(file.OpenReadStream()), property.Name, file.FileName);
                        }
                    }
                    else
                    {
                        content.Add(new StringContent(value == null ? "" : value.ToString()), property.Name);
                    }
                }
                message.Content = content;
            }
            else
            {
                if (apiRequest.Data != null)
                {
                    //If the ApiRequest.Data property is not null, it serializes the data object to JSON using JsonConvert.SerializeObject and sets it as the content of the HttpRequestMessage using StringContent.
                    message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data), Encoding.UTF8, "application/json");
                }
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

            return message;
        }
    }
}
