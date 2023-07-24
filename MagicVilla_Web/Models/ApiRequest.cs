using Microsoft.AspNetCore.Mvc;
using static MagicVilla_Utility.StaticDetails;

namespace MagicVilla_Web.Models
{
    public class ApiRequest
    {
        //specifying the HTTP method
        public ApiType ApiType { get; set; } = ApiType.GET;

        //the URL of the API endpoint
        public string ApiUrl { get; set; }

        //optional data to be sent with the request
        public object Data { get; set; }

        //
        public string Token { get; set; }

        //
        public ContentType ContentType { get; set; } = ContentType.Json;
    }
}
