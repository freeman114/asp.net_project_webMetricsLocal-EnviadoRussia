using System.Collections.Generic;

namespace InfluencersMetricsService.Model
{
    public class ValueStoryInsights
    {
        public int value { get; set; }
    }

    public class DatumStoryInsights
    {
        public string name { get; set; }
        public string period { get; set; }
        public List<ValueStoryInsights> values { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string id { get; set; }
    }

    public class StoryInsights
    {
        public List<DatumStoryInsights> data { get; set; }
    }

    public class ResumeStoryMidia
    {
        public string name { get; set; }
        public string period { get; set; }
        public List<ValueStoryInsights> values { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string id { get; set; }
    }
}
