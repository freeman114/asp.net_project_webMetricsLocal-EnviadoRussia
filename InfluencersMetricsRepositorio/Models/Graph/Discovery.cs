using System;
using System.Collections.Generic;

namespace webMetrics.Models.Graph
{
    public class DatumDisc
    {
        public int comments_count { get; set; }
        public int like_count { get; set; }
        public string caption { get; set; }
        public string media_url { get; set; }
        public DateTime timestamp { get; set; }
        public string id { get; set; }
    }

    public class CursorsDisc
    {
        public string after { get; set; }
    }

    public class PagingDisc
    {
        public CursorsDisc cursors { get; set; }
    }

    public class MediaDisc
    {
        public List<DatumDisc> data { get; set; }
        public PagingDisc paging { get; set; }
    }

    public class BusinessDiscoveryDisc
    {
        public int followers_count { get; set; }
        public int media_count { get; set; }
        public MediaDisc media { get; set; }
        public string profile_picture_url { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string biography { get; set; }
        public string id { get; set; }
    }

    public class Discovery
    {
        public BusinessDiscoveryDisc business_discovery { get; set; }
        public bool EmotionalProcessed { get; set; }
        public string id { get; set; }
        public bool? reprocessado { get; set; }
    }
}