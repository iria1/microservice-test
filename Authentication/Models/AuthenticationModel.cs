using Authentication.DBContexts;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Models
{
    public class AuthenticationModel
    {
        private readonly AuthenticationDBContext _dbContext;

        public AuthenticationModel(AuthenticationDBContext context)
        {
            _dbContext = context;
        }

        public List<AuthenticateResponse> Authenticate(AuthenticateRequest request)
        {
            return _dbContext.MasterAuth
                .Where(a => a.username == request.Username)
                .Where(b => b.password == request.Password)
                .Select(a => new AuthenticateResponse{ user_id = a.user_id })
                .ToList();
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
    }
}
