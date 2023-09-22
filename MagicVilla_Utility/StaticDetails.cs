namespace MagicVilla_Utility
{
    public class StaticDetails
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT, 
            DELETE
        }

        public static string AccessToken = "JWTToken";
        public static string RefreshToken = "JWTToken";
        public static string CurrentAPIVersion = "v2";
        public static string Admin = "admin";
        public static string Customer = "customer";

        public enum ContentType
        {
            Json,
            MultipartFormData 
        }
    }
}