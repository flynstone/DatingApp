using Api.Entities;
using Api.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public class TokenService : ITokenService
    {
        // Generate the key used for the encryption.
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser user)
        {
            // Generate claims for the token.
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.UserName)
            };

            // Generate credentials.
            var cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // Describe the token.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = cred
            };

            // Token handler.
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Create the token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Write the token and return.
            return tokenHandler.WriteToken(token);
        }
    }
}
