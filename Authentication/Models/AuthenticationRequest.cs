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

    public class ChangePasswordRequest
    {
        public long UserId { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }

    public class DeleteAccountRequest
    {
        public long UserId { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }

    public class LogoutRequest
    {
        public long UserId { get; set; }

        public string RefreshToken { get; set; }

        public LogoutType LogoutType { get; set; }
    }

    public enum LogoutType
    {
        ThisKey,            // only revoke this refresh key
        AllExceptThisKey,   // revoke all refresh keys except this key
        AllKeys             // revoke all refresh keys associated with this user
    }
}
