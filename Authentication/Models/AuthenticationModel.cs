using Authentication.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Authentication.Models
{
    public class AuthenticationModel
    {
        private readonly AuthenticationDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthenticationModel(IConfiguration configuration, AuthenticationDBContext context)
        {
            _configuration = configuration;
            _dbContext = context;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest request)
        {
            List<GetMasterAuthInfo> ar = _dbContext.MasterAuth
                .Where(a => a.username == request.Username)
                .Where(b => b.password == request.Password)
                .Select(a => new GetMasterAuthInfo { user_id = a.user_id })
                .ToList();

            string jwt = null;
            if (ar.Count > 0)
            {
                jwt = GenerateToken(ar[0].user_id.ToString());
            }

            return new AuthenticateResponse { token = jwt };
        }

        public CreateNewResponse CreateNew(CreateNewRequest request)
        {
            try
            {
                _dbContext.MasterAuth
                    .Add(new MasterAuth
                    {
                        user_id = request.UserId,
                        username = request.Username,
                        password = request.Password
                    });

                _dbContext.SaveChanges();

                return new CreateNewResponse { message = "success" };
            }
            catch (DbUpdateException ex)
            {
                return new CreateNewResponse { message = ex.InnerException.Message };
            }
        }

        private string GenerateToken(string user)
        {
            var claims = new[] {
                new Claim("userId", user)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Issuer"], claims, expires: DateTime.Now.AddMinutes(120), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
