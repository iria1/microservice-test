namespace Authentication.Models
{
    public class AuthenticateRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class CreateNewRequest
    {
        public long UserId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
