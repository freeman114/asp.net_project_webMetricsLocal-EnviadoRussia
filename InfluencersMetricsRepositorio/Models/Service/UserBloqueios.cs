using System.Collections.Generic;

namespace InfluencersMetricsService.Model
{
    public class UserBloqueios
    {
        public string AccessToken { get; set; }
        public string Description { get; set; }
        public bool Retry { get; set; }
        public string Status { get; set; }
    }
}
