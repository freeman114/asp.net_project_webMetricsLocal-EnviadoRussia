using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluencersMetricsService.Model
{
    public class Response<T>
    {
        public T Obj { get; set; }
        public string userId { get; set; }
        public string access_token { get; set; }
        public string instagram_business_account{ get; set; }
        public string id { get; set; }
    }
}
