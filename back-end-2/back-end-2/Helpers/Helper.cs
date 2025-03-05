using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace back_end_2.Helpers
{
    public static class Helper
    {
        private static readonly string[] ValidImageFormats = { ".jpg", ".jpeg", ".png" };

        // Validate file format
        public static bool IsValidImageFormat(IFormFile file)
        {
            var fileFormat = Path.GetExtension(file.FileName).ToLowerInvariant();
            return ValidImageFormats.Contains(fileFormat);
        }

        // Convert 
        public static async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        // Get roles from JWT token
        public static string[] GetRolesFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Array.Empty<string>();
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var rolesClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "roles");
            var roles = rolesClaim?.Value.Split(',') ?? Array.Empty<string>();

            return roles;
        }

        // jméno
        public static string GetUsernameFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var usernameClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "sub");
            return usernameClaim?.Value ?? string.Empty;
        }

        

        // Vytvoření tokenu
        public static string CreateToken(string username, IList<string> roles, string tokenSecret)
        {
            // define the claims obsahující informace o uživateli
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
            };

            // přidání uživatelských rolí do claims
            claims.Add(new Claim("roles", string.Join(",", roles))); // Adding roles claim

            // vytvoření bezpečnostního klíče pro podepisování tokenu
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
            //defenuju podepisovací algoritmus
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //vytvoření tokenu
            var token = new JwtSecurityToken(
                claims: claims,                       //seznam klaimu
                expires: DateTime.Now.AddMinutes(30), // expirace tokenu
                signingCredentials: creds);           // podpis a algoritmus

            // vygenerování tokenu a převod na řetězec
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
