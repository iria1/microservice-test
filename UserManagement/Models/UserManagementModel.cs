using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.DBContexts;
using Microsoft.Extensions.Configuration;

namespace UserManagement.Models
{
    public class UserManagementModel
    {
        private readonly UserManagementDBContext _dbContext;
        private readonly IConfiguration _configuration;

        public UserManagementModel(UserManagementDBContext context, IConfiguration configuration)
        {
            _dbContext = context;
            _configuration = configuration;
        }

        public List<MasterUser> GetUser(GetUserRequest request)
        {
            return _dbContext.MasterUser
                .Where(a => a.id == request.Id)
                .ToList();
        }

        public async Task<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            MasterUser newUser = new MasterUser
            {
                fullname = request.Fullname
            };

            try
            {
                _dbContext.MasterUser.Add(newUser);
                _dbContext.SaveChanges();

                var options = new RestClientOptions("https://localhost:5001/api")
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };

                var client = new RestClient(options);

                var restreq = new RestRequest("Authentication/CreateNew")
                    .AddJsonBody(new
                    {
                        UserId = newUser.id,
                        Username = request.Username,
                        Password = request.Password
                    })
                    .AddHeader("x-api-key", _configuration["apiKey"]);

                var response = await client.PostAsync<Foo>(restreq);

                if (response.message != "success")
                {
                    _dbContext.MasterUser.Remove(newUser);
                    _dbContext.SaveChanges();

                    return new CreateUserResponse { message = "failed to create login details: " + response.message };
                }

                return new CreateUserResponse { message = "success" };
            }
            catch (Exception ex)
            {
                _dbContext.MasterUser.Remove(newUser);
                _dbContext.SaveChanges();

                return new CreateUserResponse { message = ex.Message };
            }
        }

        public object ModifyUser(ModifyUserRequest request)
        {
            try
            {
                MasterUser mu = _dbContext.MasterUser
                    .Where(a => a.id == request.UserId)
                    .FirstOrDefault();

                if (mu == null) return new { message = "failed" };

                mu.fullname = request.Fullname;
                mu.modified_by = request.UserId.ToString();
                mu.modified_date = DateTime.Now;

                _dbContext.SaveChanges();

                return new { message = "success" };
            }
            catch
            {
                return null;
            }
        }

        public async Task<object> DeleteUser(DeleteUserRequest request)
        {
            try
            {
                var options = new RestClientOptions("https://localhost:5001/api")
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };

                var client = new RestClient(options);

                var restreq = new RestRequest("Authentication/DeleteAccount")
                    .AddJsonBody(new
                    {
                        UserId = request.UserId
                    })
                    .AddHeader("x-api-key", _configuration["apiKey"]);

                var response = await client.DeleteAsync<Foo>(restreq);

                if (response == null) return new { message = "failed" };

                MasterUser mu = _dbContext.MasterUser
                    .Where(a => a.id == request.UserId)
                    .FirstOrDefault();

                if (mu == null) return new { message = "failed" };

                _dbContext.MasterUser.Remove(mu);

                _dbContext.SaveChanges();

                return new { message = "success" };
            }
            catch
            {
                return null;
            }
        }
    }
}
