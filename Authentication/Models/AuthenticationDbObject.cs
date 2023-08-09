using System;

namespace Authentication.Models
{
    public class MasterAuth
    {
        public long id { get; set; }

        public long user_id { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string created_by { get; set; }

        public DateTime created_date { get; set; }

        public string modified_by { get; set; }

        public DateTime? modified_date { get; set; }

        public bool is_active { get; set; }
    }
}
