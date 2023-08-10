using Authentication.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
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
            try
            {
                var pwSalt = _dbContext.MasterAuth
                    .Where(a => a.username == request.Username)
                    .Select(a => a.pw_salt )
                    .FirstOrDefault();

                if (pwSalt == null) return new AuthenticateResponse { token = null };

                var hashedPw = ComputeSha256Hash(request.Password + pwSalt);

                MasterAuth ma = _dbContext.MasterAuth
                    .Where(a => a.username == request.Username)
                    .Where(b => b.password == hashedPw)
                    .FirstOrDefault();

                string jwt = null;
                if (ma != null)
                {
                    jwt = GenerateToken(ma.user_id.ToString());
                }

                return new AuthenticateResponse { token = jwt };
            }
            catch
            {
                return null;
            }
        }

        public CreateNewResponse CreateNew(CreateNewRequest request)
        {
            try
            {
                Guid g = Guid.NewGuid();
                var hashedPW = ComputeSha256Hash(request.Password + g.ToString());

                _dbContext.MasterAuth
                    .Add(new MasterAuth
                    {
                        user_id = request.UserId,
                        username = request.Username,
                        password = hashedPW,
                        pw_salt = g.ToString(),
                        created_by = request.UserId.ToString()
                    });

                _dbContext.SaveChanges();

                return new CreateNewResponse { message = "success" };
            }
            catch (DbUpdateException ex)
            {
                return new CreateNewResponse { message = ex.InnerException.Message };
            }
        }

        public object ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                // confirm that this user owns the account

                var pwSalt = _dbContext.MasterAuth
                    .Where(a => a.user_id == request.UserId)
                    .Select(a => a.pw_salt)
                    .FirstOrDefault();

                if (pwSalt == null) return new { message = "Failed to update password" };

                var hashedPw = ComputeSha256Hash(request.OldPassword + pwSalt);

                MasterAuth foo = _dbContext.MasterAuth
                    .Where(a => a.user_id == request.UserId)
                    .Where(b => b.password == hashedPw)
                    .FirstOrDefault();

                if (foo == null) return new { message = "Failed to update password" };

                // change the password to a new one

                Guid g = Guid.NewGuid();
                var newHashedPw = ComputeSha256Hash(request.NewPassword + g.ToString());

                foo.password = newHashedPw;
                foo.pw_salt = g.ToString();
                foo.modified_by = request.UserId.ToString();
                foo.modified_date = DateTime.Now;
                _dbContext.SaveChanges();

                return new { message = "success" };
            }
            catch
            {
                return null;
            }
        }

        public object DeleteAccount(DeleteAccountRequest request)
        {
            try
            {
                var userId = request.UserId;

                var acctToBeDelete = _dbContext.MasterAuth
                    .Where(a => a.user_id == userId)
                    .FirstOrDefault();

                if (acctToBeDelete == null) return null;

                _dbContext.MasterAuth.Remove(acctToBeDelete);

                _dbContext.SaveChanges();

                return new { message = "successfully deleted account" };
            }
            catch
            {
                return null;
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

        private string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256
            using SHA256 sha256Hash = SHA256.Create();
            // ComputeHash - returns byte array
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
