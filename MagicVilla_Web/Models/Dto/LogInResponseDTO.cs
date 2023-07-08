namespace MagicVilla_Web.Models.Dto
{
    public class LogInResponseDTO
    {
        public UserDTO LocalUser { get; set; }    
        public string? Token { get; set; }
    }
}
