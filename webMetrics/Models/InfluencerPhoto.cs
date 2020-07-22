using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace webMetrics.Models
{
    [BsonIgnoreExtraElements]
    public class InfluencerPhoto
    {
        public BsonObjectId _id { get; set; }
        public string Name { get; set; }
        public string UrlPhoto { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public string Followers { get; set; }
        public string Followings { get; set; }
        public string Posts { get; set; }
        public int Type { get; set; }
        public DateTime DateProcesss { get; set; }
        public List<PictureInf> Photos { get; set; }
        public bool Processed { get; set; }
        public string Categorized { get; set; }
        public bool CategoryML { get; set; }
        public string CategoryDescriptionML { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class VideoYoutube
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Caption { get; set; }
        public string CategoryDescription { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Twett
    {
        //public long TweetID { get; set; }
        public string Description { get; set; }
        public string URLPhoto { get; set; }
        public string CategoryDescription { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class InfluencerPhotoYoutube
    {
        public BsonObjectId _id { get; set; }
        public string Channel { get; set; }
        public List<VideoYoutube> Videos { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class InfluencerTwitter
    {
        public BsonObjectId _id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Followers { get; set; }
        public Twett[] Tweets { get; set; }
    }
}
