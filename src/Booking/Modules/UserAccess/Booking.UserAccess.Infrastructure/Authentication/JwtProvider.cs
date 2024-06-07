﻿using Booking.UserAccess.Application.Abstractions;
using Booking.UserAccess.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Booking.UserAccess.Infrastructure.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private JwtOptions _jwtOptions;

        public JwtProvider(IOptions<JwtOptions> options)
        {
            _jwtOptions = options.Value;
        }
        public string Generate(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,user.Email)
            };

            foreach (var role in user.Roles)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role.Name));
            }


            var permisions = user.Roles.SelectMany(x => x.Permissions);

            foreach (var permission in permisions)
            {
                claims.Add(
                    new Claim("permission", permission.Name));
            }

            SigningCredentials signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                SecurityAlgorithms.HmacSha256
            );

            JwtSecurityToken token = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                null,
                DateTime.UtcNow.AddMinutes(30),
                signingCredentials
            );

            string tokenContent = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenContent;
        }
    }
}
