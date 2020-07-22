using System;

namespace webMetrics.Models
{
    public class AccessToken
    {
        public class User
        {
            public string id { get; set; }
            public string username { get; set; }
            public string profile_picture { get; set; }
            public string full_name { get; set; }
            public string bio { get; set; }
            public string website { get; set; }
            public bool is_business { get; set; }
        }

            public string access_token { get; set; }
            public User user { get; set; }
        
    }
}