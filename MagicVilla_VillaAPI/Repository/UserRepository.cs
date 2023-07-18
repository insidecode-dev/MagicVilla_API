using AutoMapper;
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
        private string? secretKey;

        // after identity 
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserRepository(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _dbContext
                       .ApplicationUsers
                       .FirstOrDefault(x => x.UserName == username);

            if (user == null) { return true; }
            return false;
        }

        public async Task<LogInResponseDTO> LogIn(LogInRequestDto logInRequestDTO)
        {
            ApplicationUser? user = await _dbContext
                             .ApplicationUsers
                             .FirstOrDefaultAsync(x=>x.UserName.ToLower()==logInRequestDTO.UserName.ToLower());            

            // checking if the password of user found by username is same with password in requestlogin
            var isValidPassword = await _userManager.CheckPasswordAsync(user,logInRequestDTO.Password);

            // validating all two checks above 
            if (user is null || !isValidPassword)
            {
                return new LogInResponseDTO()
                {
                    LocalUser = null,
                    Token = ""
                };
            }

            // we use user's role when generating token inside tokenDescription, that's why we retrieve user's role
            var roles = await _userManager.GetRolesAsync(user);

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
                        new Claim(ClaimTypes.Name, user.UserName.ToString()),                        
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                
                // finally we create symmetric security key with our key variable 
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            // generating token 
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor); // after adding identity I extracted role from token itself

            //
            return new LogInResponseDTO()
            {
                LocalUser = _mapper.Map<UserDTO>(user),
                
                //the line below will generate the token that we'll finally use
                Token = tokenHandler.WriteToken(token)
                //Role = roles.FirstOrDefault()
            };
        }

        public async Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            // automapper could be used here
            //LocalUser localUser = new()
            //{
            //    Name = logInRequestDTO.Name,
            //    UserName = logInRequestDTO.UserName,
            //    Password = logInRequestDTO.Password,
            //    Role = logInRequestDTO.Role
            //};

            // after identity
            ApplicationUser localUser = new()
            {
                UserName = registrationRequestDTO.UserName,
                Email = registrationRequestDTO.UserName,
                NormalizedEmail = registrationRequestDTO.UserName.ToUpper(),
                Name = registrationRequestDTO.Name                
            };

            try
            {
                // the password below is not a hashed password, we'll just write the password and it will automatically hash that and create the user 
                var result = await _userManager.CreateAsync(user:localUser,password:registrationRequestDTO.Password);

                // flag indicating whether if the operation succeeded or not 
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(registrationRequestDTO.Role).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registrationRequestDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user:localUser, role:registrationRequestDTO.Role);

                    var createdUser = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(x=>x.UserName==registrationRequestDTO.UserName);

                    return _mapper.Map<UserDTO>(createdUser);
                }

                // add error message if this is not successful
            }
            catch (Exception)
            { 
                
            }
           
            return new UserDTO();
        }
    }
}
