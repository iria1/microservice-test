using UserManagement.DBContexts;
using System.Collections.Generic;
using System.Linq;

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
    }
}
