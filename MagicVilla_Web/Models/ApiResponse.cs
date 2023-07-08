using System.Net;

namespace MagicVilla_Web.Models
{
    public class ApiResponse
    {
        public ApiResponse() { 
        ErrorMessages = new List<string>();
        }
        //the HTTP status code
        public HttpStatusCode StatusCode { get; set; }

        //indicating whether the request was successful
        public bool IsSuccess { get; set; } = true;

        //a list of error messages if the request failed
        public List<string> ErrorMessages { get; set; }

        //the actual response data
        public object Result { get; set; }
    }
}
