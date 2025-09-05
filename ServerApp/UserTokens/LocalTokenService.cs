using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
namespace ServerApp.UserTokens {
    public class LocalTokenService
    {
        private readonly IConfiguration _config;

        public LocalTokenService(IConfiguration config)
        {
            _config = config;
        }

        //local custom token to be sent to the API
        public string CreateApiToken(ClaimsPrincipal user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["LocalToken:SigningKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Copy claims from the logged-in user
            var claims = new List<Claim>(user.Claims);



            var token = new JwtSecurityToken(
                issuer: _config["LocalToken:Issuer"],
                audience: _config["LocalToken:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
