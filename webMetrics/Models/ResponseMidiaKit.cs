using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webMetrics.Models
{
    public class ResponseMidiaKit
    {
        public string UserName { get; set; }
        public long Followers { get; set; }
        public string UrlImage { get; set; }
        public List<Gender> Genders { get; set; }

        public List<City> Cities { get; set; }
        public int Engagement { get; set; }
        public int ReachWeek { get; set; }
        public int ViewsStory { get; set; }
        public List<PostsTop> PostsTops { get; set; }

    }

    public class Gender
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class City
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class PostsTop
    {
        public string UrlImage { get; set; }
        public int Likes { get; set; }
        public int Engagement { get; set; }
        public string Caption { get; set; }
    }

}
