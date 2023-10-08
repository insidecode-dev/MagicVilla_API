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

        public async Task<TokenDTO> LogIn(LogInRequestDto logInRequestDTO)
        {
            ApplicationUser? user = await _dbContext
                             .ApplicationUsers
                             .FirstOrDefaultAsync(x => x.UserName.ToLower() == logInRequestDTO.UserName.ToLower());

            // checking if the password of user found by username is same with password in requestlogin
            var isValidPassword = await _userManager.CheckPasswordAsync(user, logInRequestDTO.Password);

            // validating all two checks above 
            if (user is null || !isValidPassword)
            {
                return new TokenDTO()
                {
                    AccessToken = ""
                };
            }

            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GetAccessToken(user, jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);

            //
            return new TokenDTO()
            {
                AccessToken = accessToken,
                //Role = roles.FirstOrDefault()
                RefreshToken = refreshToken
            };
        }

        private async Task<string> GetAccessToken(ApplicationUser applicationUser, string jwtTokenId)
        {
            // we use user's role when generating token inside tokenDescription, that's why we retrieve user's role
            var roles = await _userManager.GetRolesAsync(applicationUser);

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
                        new Claim(ClaimTypes.Name, applicationUser.UserName.ToString()),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId), // jti represents unique id for jwt token
                        new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id)
                    }),
                Expires = DateTime.UtcNow.AddMinutes(1),           
                
                // finally we create symmetric security key with our key variable 
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
             

            // generating token 
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor); // after adding identity I extracted role from token itself

            //the line below will generate the token that we'll finally use
            var token = tokenHandler.WriteToken(securityToken);

            return token;
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
                var result = await _userManager.CreateAsync(user: localUser, password: registrationRequestDTO.Password);

                // flag indicating whether if the operation succeeded or not 
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(registrationRequestDTO.Role).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registrationRequestDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user: localUser, role: registrationRequestDTO.Role);

                    var createdUser = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == registrationRequestDTO.UserName);

                    return _mapper.Map<UserDTO>(createdUser);
                }

                // add error message if this is not successful
            }
            catch (Exception)
            {

            }

            return new UserDTO();
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            // find an existing refresh token
            var existingRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null)
            {
                return new TokenDTO();
            }

            // compare data from existing refresh and access token provided and if there is any missmatch then consider it as a fraud
            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId,existingRefreshToken.JwtTokenId);
            if (!isTokenValid)  
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // when someone tries to use not valid refresh token, fraud possible
            if (!existingRefreshToken.IsValid)
            {
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            }

            // If just expired then mark as invalid and return empty
            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // replace old refresh token with a new one with updated expire date 
            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            // revoke existing refresh token and mark existing as invalid
            await MarkTokenAsInvalid(existingRefreshToken); 

            // generate new access token
            var applicationUser = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == existingRefreshToken.UserId);
            if (applicationUser == null)
            {
                return new TokenDTO();
            }

            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

            return new TokenDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }

        private async Task<string> CreateNewRefreshToken(string userId, string jwtTokenId)
        {
            RefreshToken refreshToken = new()
            {
                IsValid = true,
                JwtTokenId = jwtTokenId,  
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid()
            };
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);

                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return userId==expectedUserId && jwtTokenId==expectedTokenId;
            }
            catch 
            {
                return false;
            }
        }

        public async Task RevokefreshToken(TokenDTO tokenDTO)
        {
            var existingRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Refresh_Token == tokenDTO.RefreshToken);

            if (existingRefreshToken == null)
                return;            

            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {                
                return;
            }

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        }

        // helper methods
        private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
        {
            await _dbContext.RefreshTokens.Where(u => u.UserId == userId && u.JwtTokenId == tokenId)
                    .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false)); // new feature, bulk update 

            //foreach (var item in chainRecords)
            //{
            //    item.IsValid = false;
            //}
            //_dbContext.UpdateRange(chainRecords);
            //await _dbContext.SaveChangesAsync();            
        }

        private async Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
            await _dbContext.SaveChangesAsync();
        }

        
    }
}