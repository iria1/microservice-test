using EFCoreMySQL.DBContexts;
using System.Collections.Generic;
using System.Linq;

namespace Authentication.Models
{
    public class AuthenticationModel
    {
        private readonly MyDBContext myDbContext;

        public AuthenticationModel(MyDBContext context)
        {
            myDbContext = context;
        }

        public List<MasterAuth> Authenticate(AuthenticateRequest request)
        {
            return myDbContext.MasterAuth
                .Where(a => a.username == request.Username)
                .Where(b => b.password == request.Password)
                .ToList();
        }
    }
}
