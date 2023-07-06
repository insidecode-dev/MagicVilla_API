using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private string secretKey;
        public UserRepository(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            secretKey= configuration.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueUser(string username)
        {
            var user = _dbContext.LocalUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null) { return true; }
            return false;
        }

        public async Task<LogInResponseDTO> LogIn(LogInRequestDto logInRequestDTO)
        {
            var user = await _dbContext.LocalUsers.FirstOrDefaultAsync(x=>x.UserName==logInRequestDTO.UserName && x.Password==logInRequestDTO.Password);
            if (user is null)
            {
                return new LogInResponseDTO()
                {
                    LocalUser = null,
                    Token = ""
                };
            }
            
            //
            var tokenHandler = new JwtSecurityTokenHandler();

            // this line converts secret key to bytes and we'll have that as byte array in the variable => key
            var key = Encoding.ASCII.GetBytes(secretKey);

            // tokenDescriptor basically contains everything like what are all the claims in a token
            // Claim - this will basically identify that this is name of the user, this is the role that you have,    
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),                        
                        new Claim(ClaimTypes.Role, user.Role),
                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                
                // finally we create symmetric security key with our key variable 
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                
            };


            // generating token 
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            //
            return new LogInResponseDTO()
            {
                LocalUser = user,
                
                //the line below will generate the token that we'll finally use
                Token = tokenHandler.WriteToken(token) 
            };
        }

        public async Task<LocalUser> Register(RegistrationRequestDTO logInRequestDTO)
        {
            // automapper could be used here
            LocalUser localUser = new()
            {
                Name = logInRequestDTO.Name,
                UserName = logInRequestDTO.UserName,
                Password = logInRequestDTO.Password,
                Role = logInRequestDTO.Role
            };

            await _dbContext.LocalUsers.AddAsync(localUser);
            await _dbContext.SaveChangesAsync();
            localUser.Password = "";
            return localUser;
        }
    }
}
