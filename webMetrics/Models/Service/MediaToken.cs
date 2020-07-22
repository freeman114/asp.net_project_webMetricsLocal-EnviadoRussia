using System;
using System.Collections.Generic;

namespace InfluencersMetricsService.Model
{
    
    public class MediaToken
    {
        public List<webMetrics.Models.Graph.DatumDisc> data { get; set; }
        public string IdDiscovery {get;set;}
        public string Token {get;set;}
        public string Account_Business_Instagram {get;set;}
    }
    
}
