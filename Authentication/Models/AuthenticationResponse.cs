﻿namespace Authentication.Models
{
    public class AuthenticateResponse
    {
        public string token { get; set; }
    }

    public class GetMasterAuthInfo
    {
        public long user_id { get; set; }
    }

    public class CreateNewResponse
    {
        public string message { get; set; }
    }
}
