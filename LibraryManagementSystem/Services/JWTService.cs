using LibraryManagementSystem.Data.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagementSystem.API.Services
{
    public class JWTService
    {
        private readonly IConfiguration config;
        private readonly SymmetricSecurityKey jwtKey;

        public JWTService(IConfiguration config)
        {
            this.config = config;
            this.jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));
        }

        public string CreateJWT(User user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.GivenName,user.FirstName),
                new Claim(ClaimTypes.Surname,user.LastName)
            };
            var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddHours(int.Parse(config["JWT:ExpiresInHours"])),
                SigningCredentials = credentials,
                Issuer = config["JWT:Issuer"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt=tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(jwt);
        }
    }
}
