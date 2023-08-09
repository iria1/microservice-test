namespace UserManagement.Models
{
    public class GetUserRequest
    {
        public long Id { get; set; }
    }

    public class CreateUserRequest
    {
        public string Fullname { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
