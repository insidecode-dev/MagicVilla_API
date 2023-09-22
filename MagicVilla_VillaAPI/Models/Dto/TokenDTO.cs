namespace MagicVilla_VillaAPI.Models.Dto
{
    public class TokenDTO
    {
        // I changed type of LocalUser property from LocalUser to UserDTO, because after we use Identity many of things comes built-in Identity and do not need to add other properties anymore
        // I do not use the type ApplicationUser directky because it has more properties thos we don't need to use in response 
        //public UserDTO? LocalUser { get; set; }    
        public string AccessToken { get; set; }
        //public string? Role { get; set; } //I commented this because we'll retrieve role from token itself 
        public string RefreshToken { get; set; }
    }
}
