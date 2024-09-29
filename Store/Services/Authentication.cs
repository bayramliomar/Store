using Microsoft.IdentityModel.Tokens;
using Store.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Store.Services
{
    public class Authentication
    {

        public static string GenerateJWTAuthetication(string userName, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtHeaderParameterNames.Jku, userName),
                new Claim(JwtHeaderParameterNames.Kid, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userName)
            };

            claims.Add(new Claim(ClaimTypes.Role, string.Join(",", roles)));


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Convert.ToString(ConfigurationManager.AppSettings["config:JwtKey"])));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires =
                DateTime.Now.AddMinutes(
                    Convert.ToDouble(Convert.ToString(ConfigurationManager.AppSettings["config:JwtExpireMinutes"])));

            var token = new JwtSecurityToken(
                Convert.ToString(ConfigurationManager.AppSettings["config:JwtIssuer"]),
                Convert.ToString(ConfigurationManager.AppSettings["config:JwtAudience"]),
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);


        }


        public static JwtSecurityToken ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Convert.ToString(ConfigurationManager.AppSettings["config:JwtKey"]));
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero

                }, out SecurityToken validatedToken);

                // Corrected access to the validatedToken
                var jwtToken = (JwtSecurityToken)validatedToken;
                var jku = jwtToken.Claims.First(claim => claim.Type == "jku").Value;
                var userName = jwtToken.Claims.First(claim => claim.Type == "kid").Value;

                return jwtToken;
            }
            catch
            {
                return null;
            }
        }

        public static string[] GetRolesFromToken(string token)
        {
            // Create a JwtSecurityTokenHandler to read the token
            var handler = new JwtSecurityTokenHandler();

            // Validate the token and read its claims
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken != null)
                {
                    // Get the role claims
                    var roleClaims = jwtToken.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToArray();
                    List<string> rolesList = new List<string>(roleClaims);
                    return rolesList[0].Split(','); // Return the roles
                }
            }

            return new string[] { }; // Return an empty array if no roles are found or if the token is invalid
        }

    }

}