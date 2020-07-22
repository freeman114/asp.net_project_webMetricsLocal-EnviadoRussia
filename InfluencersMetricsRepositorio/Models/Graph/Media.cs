using System;
using System.Collections.Generic;

namespace webMetrics.Models.Graph
{
    public class Datum2
    {
        public string username { get; set; }
        public string text { get; set; }
        public string id { get; set; }
    }

    public class Comments
    {
        public List<Datum2> data { get; set; }
    }

    public class Owner
    {
        public string id { get; set; }
    }

    public class Datum3
    {
        public string id { get; set; }
    }

    public class Children
    {
        public List<Datum3> data { get; set; }
    }

    public class Datum
    {
        public string caption { get; set; }
        public Comments comments { get; set; }
        public int comments_count { get; set; }
        public string id { get; set; }
        public string ig_id { get; set; }
        public bool is_comment_enabled { get; set; }
        public int like_count { get; set; }
        public string media_type { get; set; }
        public string media_url { get; set; }
        public Owner owner { get; set; }
        public string permalink { get; set; }
        public string shortcode { get; set; }
        public DateTime timestamp { get; set; }
        public string username { get; set; }
        public Children children { get; set; }

        public bool Insight { get; set; } = false;

        public int Impressions { get; set; }
        public int Reach { get; set; }
        public int Saved { get; set; }
        public int Engagement { get; set; }
    }

    public class Cursors
    {
        public string before { get; set; }
        public string after { get; set; }
    }

    public class Paging
    {
        public Cursors cursors { get; set; }
        public string next { get; set; }
    }

    public class Media
    {
        public List<Datum> data { get; set; }
        public Paging paging { get; set; }
    }

    public class Tags
    {
        public List<Datum> data { get; set; }
        public Paging paging { get; set; }
    }

    public class Stories
    {
        public List<Datum> data { get; set; }
        public Paging paging { get; set; }
    }
}