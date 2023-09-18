namespace Authentication.Models
{
    public class AuthenticateResponse
    {
        public string auth_token { get; set; }

        public string refresh_token { get; set; }
    }

    public class CreateNewResponse
    {
        public string message { get; set; }
    }
}
