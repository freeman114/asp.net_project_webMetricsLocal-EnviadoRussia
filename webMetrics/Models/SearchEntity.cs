using MongoDB.Bson;
using System.Collections.Generic;

namespace webMetrics.Models
{
    public class SearchEntity
    {
        public SearchEntity()
        {
        }
        public BsonObjectId _id { get; set; }
        public string WordSearch { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public long CountAllResults { get; set; } = 1000000;
        public int DisplayLenght { get; set; } = 10;
        public List<int> PaginationButtonsNameValue { get; set; } = new List<int>() { 1 };
        public List<InfluencerEntity> Influencers { get; set; } = new List<InfluencerEntity>();
        public string ResultsInTime { get; set; }
        public bool FilterByInstagram { get; set; } = true;
        public bool FilterByYoutube { get; set; } = true;
        public bool FilterByTwitter { get; set; } = true;
        public bool FilterByTiktopk { get; set; } = true;
        public bool FilterByLinkedin { get; set; } = true;
        public bool FilterByPodCasts { get; set; } = true;
        public int? Age { get; set; }
        public string Gender { get; set; }
        public List<string> Categories { get; set; }
    }
}
