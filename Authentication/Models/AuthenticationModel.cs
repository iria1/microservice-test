using Authentication.DBContexts;
using CommonClass;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace Authentication.Models
{
    public class AuthenticationModel
    {
        private readonly ILogger _logger;
        private readonly AuthenticationDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthenticationModel(ILogger logger, IConfiguration configuration, AuthenticationDBContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = context;
        }

        public CommonResponse<AuthenticateResponse> Authenticate(AuthenticateRequest request)
        {
            try
            {
                MasterAuth ma = _dbContext.MasterAuth.Where(a => a.username == request.Username).FirstOrDefault();
                if (ma == null) return new CommonResponse<AuthenticateResponse>(null, "Wrong username/password combination.", false);

                var hashedPw = ma.password;

                if (BC.EnhancedVerify(request.Password, hashedPw))
                {
                    string authJwt = GenerateToken(ma.user_id.ToString(), DateTime.Now.AddMinutes(5), _configuration["Jwt:AuthKey"]);
                    string refreshJwt = GenerateToken(ma.user_id.ToString(), DateTime.Now.AddDays(30), _configuration["Jwt:RefrKey"]);

                    string signature = refreshJwt.Split('.')[^1];

                    _dbContext.MasterRefreshToken
                    .Add(new MasterRefreshToken
                    {
                        user_id = ma.user_id,
                        token = signature,
                        created_by = ma.user_id.ToString()
                    });

                    _dbContext.SaveChanges();

                    return new CommonResponse<AuthenticateResponse>(new AuthenticateResponse { auth_token = authJwt, refresh_token = refreshJwt }, null, true);
                }
                else
                {
                    return new CommonResponse<AuthenticateResponse>(null, "Wrong username/password combination.", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new CommonResponse<AuthenticateResponse>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        public CommonResponse<object> CreateNew(CreateNewRequest request)
        {
            try
            {
                string hashedPW = BC.EnhancedHashPassword(request.Password, 13);

                _dbContext.MasterAuth
                    .Add(new MasterAuth
                    {
                        user_id = request.UserId,
                        username = request.Username,
                        password = hashedPW,
                        created_by = request.UserId.ToString()
                    });

                _dbContext.SaveChanges();

                return new CommonResponse<object>(null, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        public CommonResponse<object> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                // confirm that this user owns the account

                MasterAuth ma = _dbContext.MasterAuth.Where(a => a.user_id == request.UserId).FirstOrDefault();
                if (ma == null) return new CommonResponse<object>(null, "UserId not found. Failed to update password.", false);

                var hashedPw = ma.password;
                if (!BC.EnhancedVerify(request.OldPassword, hashedPw)) return new CommonResponse<object>(null, "Incorrect password. Failed to update password.", false);

                // change the password to a new one

                string newHashedPw = BC.EnhancedHashPassword(request.NewPassword, 13);

                ma.password = newHashedPw;
                ma.modified_by = request.UserId.ToString();
                ma.modified_date = DateTime.Now;
                _dbContext.SaveChanges();

                return new CommonResponse<object>(null, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        public CommonResponse<object> DeleteAccount(DeleteAccountRequest request)
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

                return new CommonResponse<object>(null, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        public CommonResponse<AuthenticateResponse> Refresh(RefreshRequest request)
        {
            try
            {
                if (!ValidateToken(request.RefreshToken)) return new CommonResponse<AuthenticateResponse>(null, "Invalid refresh token.", false);

                string signature = request.RefreshToken.Split('.')[^1];

                var mrt = _dbContext.MasterRefreshToken.Where(a => a.token == signature)
                    .FirstOrDefault();

                if (mrt == null)
                {
                    _logger.LogCritical($"A potential breach has been detected. " +
                        $"The token in the payload is valid, but it does not belong in the list of issued tokens. " +
                        $"The secret used to sign refresh tokens may have been compromised. " +
                        $"The token in question is {request.RefreshToken}.");

                    return new CommonResponse<AuthenticateResponse>(null, "Invalid refresh token.", false);
                }
                else if (!mrt.is_active)
                {
                    _logger.LogCritical($"A potential breach has been detected. " +
                        $"The refresh token with ID {mrt.id} for userId {mrt.user_id} has been used even though it has been revoked. " +
                        $"The token in question is {request.RefreshToken}.");

                    return new CommonResponse<AuthenticateResponse>(null, "Invalid refresh token.", false);
                }
                
                string authJwt = GenerateToken(mrt.user_id.ToString(), DateTime.Now.AddMinutes(5), _configuration["Jwt:AuthKey"]);
                string refreshJwt = GenerateToken(mrt.user_id.ToString(), DateTime.Now.AddDays(30), _configuration["Jwt:RefrKey"]);

                signature = refreshJwt.Split('.')[^1];

                mrt.is_active = false;
                mrt.modified_by = mrt.user_id.ToString();
                mrt.modified_date = DateTime.Now;

                _dbContext.MasterRefreshToken
                    .Add(new MasterRefreshToken
                    {
                        user_id = mrt.user_id,
                        token = signature,
                        created_by = mrt.user_id.ToString()
                    });

                _dbContext.SaveChanges();

                return new CommonResponse<AuthenticateResponse>(new AuthenticateResponse { auth_token = authJwt, refresh_token = refreshJwt }, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new CommonResponse<AuthenticateResponse>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        public CommonResponse<object> Logout(LogoutRequest request)
        {
            try
            {
                List<MasterRefreshToken> listMrt;
                string signature = request.RefreshToken.Split('.')[^1];

                switch (request.LogoutType)
                {
                    case LogoutType.ThisKey:
                        listMrt = _dbContext.MasterRefreshToken.Where(x => x.user_id == request.UserId && x.token == signature).ToList();
                        break;
                    case LogoutType.AllExceptThisKey:
                        listMrt = _dbContext.MasterRefreshToken.Where(x => x.user_id == request.UserId && x.token != signature).ToList();
                        break;
                    case LogoutType.AllKeys:
                        listMrt = _dbContext.MasterRefreshToken.Where(x => x.user_id == request.UserId).ToList();
                        break;
                    default:
                        throw new Exception($"Invalid LogoutType used ({request.LogoutType} is used).");
                }

                foreach (var item in listMrt)
                {
                    item.is_active = false;
                    item.modified_by = request.UserId.ToString();
                    item.modified_date = DateTime.Now;
                }

                _dbContext.SaveChanges();

                return new CommonResponse<object>(null, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        private string GenerateToken(string user, DateTime duration, string key)
        {
            var claims = new[] {
                new Claim("userId", user)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Issuer"], claims, expires: duration, signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool ValidateToken(string authToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();

                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:RefrKey"]))
            };
        }
    }
}
