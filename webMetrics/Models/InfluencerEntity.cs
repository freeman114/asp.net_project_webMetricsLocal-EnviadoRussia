using MongoDB.Bson;
using System.Collections.Generic;

namespace webMetrics.Models
{
    public class InfluencerEntity
    {

        public BsonObjectId _id { get; set; }
        public string ImageProfile { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Estate { get; set; }
        public string SocialClass { get; set; }
        public string Text { get; set; }
        public string Search { get; set; }
        public string Origin { get; set; }//I=instagram,Y=youtube,T=twitter
        public int Followers { get; set; }
        public bool? ProcessedAgeGender { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public List<string> Categories { get; set; }
        public bool CategoryML { get; set; }
    }
}
