using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.DBContexts;

namespace UserManagement.Models
{
    public class UserManagementModel
    {
        private readonly UserManagementDBContext _dbContext;

        public UserManagementModel(UserManagementDBContext context)
        {
            _dbContext = context;
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

                var restreq = new RestRequest("Authentication/CreateNew").AddJsonBody(new
                {
                    UserId = newUser.id,
                    Username = request.Username,
                    Password = request.Password
                });

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
    }
}
