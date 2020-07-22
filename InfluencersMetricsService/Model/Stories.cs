using System;
using System.Collections.Generic;

namespace InfluencersMetricsService.Model
{
    public class Datum
    {
        public string id { get; set; }
        public string media_url { get; set; }
        public string permalink { get; set; }
        public string media_type { get; set; }
        public string caption { get; set; }
        public string username { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class Cursors
    {
        public string before { get; set; }
        public string after { get; set; }
    }

    public class Paging
    {
        public Cursors cursors { get; set; }
    }

    public class Stories
    {
        public List<Datum> data { get; set; }
        public Paging paging { get; set; }
        public Error Error { get; set; }
    }
}
