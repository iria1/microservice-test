using CommonClass;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.DBContexts;

namespace UserManagement.Models
{
    public class UserManagementModel
    {
        private readonly ILogger _logger;
        private readonly UserManagementDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public UserManagementModel(ILogger logger, UserManagementDBContext context, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = context;
            _configuration = configuration;
        }

        public CommonResponse<MasterUser> GetUser(GetUserRequest request)
        {
            try
            {
                MasterUser mu = _dbContext.MasterUser
                    .Where(a => a.id == request.Id)
                    .FirstOrDefault();

                if (mu != null)
                {
                    return new CommonResponse<MasterUser>(mu, null, true);
                }
                else
                {
                    return new CommonResponse<MasterUser>(null, "UserId not found.", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);

                return new CommonResponse<MasterUser>(null, "An unexpected error occurred. Please contact administrator.", true);
            }
        }

        public async Task<CommonResponse<object>> CreateUser(CreateUserRequest request)
        {
            MasterUser newUser = new MasterUser
            {
                fullname = request.Fullname
            };

            try
            {
                _dbContext.MasterUser.Add(newUser);
                _dbContext.SaveChanges();

                var response = await RestApiCall("POST", "https://localhost:5001/api", "Authentication/CreateNew", new { UserId = newUser.id, Username = request.Username, Password = request.Password }, true);

                if (response.IsSuccess == false)
                {
                    _dbContext.MasterUser.Remove(newUser);
                    _dbContext.SaveChanges();

                    return new CommonResponse<object>(null, "Failed to create login details: " + response.Message, false);
                }

                return new CommonResponse<object>(null, null, true);
            }
            catch (Exception ex)
            {
                _dbContext.MasterUser.Remove(newUser);
                _dbContext.SaveChanges();

                _logger.LogCritical(ex.Message);

                return new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        public CommonResponse<object> ModifyUser(ModifyUserRequest request)
        {
            try
            {
                MasterUser mu = _dbContext.MasterUser
                    .Where(a => a.id == request.UserId)
                    .FirstOrDefault();

                if (mu == null) return new CommonResponse<object>(null, "UserId not found.", false);

                mu.fullname = request.Fullname;
                mu.modified_by = request.UserId.ToString();
                mu.modified_date = DateTime.Now;

                _dbContext.SaveChanges();

                return new CommonResponse<object>(null, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);

                return new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        public async Task<CommonResponse<object>> DeleteUser(long userId)
        {
            try
            {
                var response = await RestApiCall("DELETE", "https://localhost:5001/api", "Authentication/DeleteAccount", new { UserId = userId }, true);

                if (response.IsSuccess == false) return new CommonResponse<object>(null, "Failed to delete login details: " + response.Message, false);

                MasterUser mu = _dbContext.MasterUser
                    .Where(a => a.id == userId)
                    .FirstOrDefault();

                if (mu == null) return new CommonResponse<object>(null, "Failed", false);

                _dbContext.MasterUser.Remove(mu);

                _dbContext.SaveChanges();

                return new CommonResponse<object>(null, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);

                return new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);
            }
        }

        private async Task<CommonResponse<object>> RestApiCall(string verb, string uri1, string uri2, object jsonBody, bool addApiKey)
        {
            CommonResponse<object> response = new CommonResponse<object>(null, "An unexpected error occurred. Please contact administrator.", false);

            try
            {
                var options = new RestClientOptions(uri1)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };

                var client = new RestClient(options);

                var restreq = new RestRequest(uri2);

                if (jsonBody != null)
                {
                    restreq.AddJsonBody(jsonBody);
                }

                if (addApiKey)
                {
                    restreq.AddHeader("x-api-key", _configuration["apiKey"]);
                }

                switch (verb)
                {
                    case "GET":
                        response = await client.GetAsync<CommonResponse<object>>(restreq);
                        break;
                    case "POST":
                        response = await client.PostAsync<CommonResponse<object>>(restreq);
                        break;
                    case "PUT":
                        response = await client.PutAsync<CommonResponse<object>>(restreq);
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync<CommonResponse<object>>(restreq);
                        break;
                    default:
                        break;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);

                return response;
            }
        }
    }
}
