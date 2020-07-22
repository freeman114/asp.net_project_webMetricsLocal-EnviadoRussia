using System.Collections.Generic;

namespace InfluencersMetricsService.Model
{
    public class ValueUserInsights
    {
        public int value { get; set; }
    }

    public class DatumUserInsights
    {
        public string name { get; set; }
        public string period { get; set; }
        public List<ValueUserInsights> values { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string id { get; set; }
    }

    public class UserInsights
    {
        public List<DatumUserInsights> data { get; set; }
    }
}
