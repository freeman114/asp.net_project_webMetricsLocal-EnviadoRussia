using System;
using System.Collections.Generic;

namespace InfluencersMetricsService.Model
{
    
    public class MediaToken
    {
        public List<DatumDisc> data { get; set; }
        public string IdDiscovery {get;set;}
        public string Token {get;set;}
        public string Account_Business_Instagram {get;set;}
    }
    public class DatumDisc
    {
        public int comments_count { get; set; }
        public int like_count { get; set; }
        public string caption { get; set; }
        public string media_url { get; set; }
        public DateTime timestamp { get; set; }
        public string id { get; set; }
    }
}
