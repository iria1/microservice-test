using System;

namespace UserManagement.Models
{
    public class MasterUser
    {
        public long id { get; set; } 

        public string fullname { get; set; }
        
        public string created_by { get; set; }

        public DateTime created_date { get; set; }

        public string modified_by { get; set; }

        public DateTime? modified_date { get; set; }

        public bool is_active { get; set; }
    }
}
