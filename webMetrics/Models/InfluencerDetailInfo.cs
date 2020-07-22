using System.Collections.Generic;

namespace webMetrics.Models
{
    public class InfluencerDetailInfo
    {
        public string UserName { get; set; }
        public string Description { get; set; }
        public string PostsCount { get; set; }
        public string Followers { get; set; }
        public string Following { get; set; }
        public string Engagement { get; set; }
        public string Impressions { get; set; }
        public string Reach { get; set; }
        public List<string> ReachTags { get; set; }
        public List<PostInfluencer> Posts { get; set; }
        public string Contacts { get; set; }
        public string ImageProfile { get; set; }
        public string Origin { get; set; }

        public InfluencerDetailInfo(
            string userName, 
            string description, 
            string postsCount, 
            string followers, 
            string following, 
            string engagement, 
            string impressions, 
            string reach, 
            List<string> reachTags, 
            List<PostInfluencer> posts, 
            string contacts, 
            string imageProfile,
            string origin)
        {
            UserName = userName;
            Description = description;
            PostsCount = postsCount;
            Followers = followers;
            Following = following;
            Engagement = engagement;
            Impressions = impressions;
            Reach = reach;
            ReachTags = reachTags;
            Posts = posts;
            Contacts = contacts;
            ImageProfile = imageProfile;
            this.Origin = origin;
        }

        //public InfluencerDetailInfo(
        //    string userName, 
        //    string description, 
        //    string postsCount, 
        //    string followers, 
        //    string following, 
        //    string engagement, 
        //    string impressions, 
        //    string reach,
        //    List<string> reachTags, 
        //    List<PostInfluencer> posts,
        //    string contacts)
        //{
        //    this.UserName = userName;
        //    this.Description = description;
        //    this.PostsCount = postsCount;
        //    this.Followers = followers;
        //    this.Following = following;
        //    this.Engagement = engagement;
        //    this.Impressions = impressions;
        //    this.Reach = reach;
        //    this.ReachTags = reachTags;
        //    this.Posts = posts;
        //    this.Contacts = contacts;
        //}

    }
}
