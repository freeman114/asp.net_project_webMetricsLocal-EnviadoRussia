namespace webMetrics.Models
{
    public class SearchInfluencersRequest
    {
        public string WordSearch { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int DisplayLenght { get; set; } = 20;
        public bool Insta { get; set; } = false;
        public bool Youtube { get; set; } = false;
        public bool Twitter { get; set; } = false;
        public bool Tiktopk { get; set; } = false;
        public bool Linkedin { get; set; } = false;
        public bool PodCasts { get; set; } = false;

        public int MinAge { get; set; } = 0;
        public int MaxAge { get; set; } = 100;
        public bool GenderMale { get; set; } = true;
        public bool GenderFemale { get; set; } = true;
        public int MinFollowers { get; set; }
        public int MaxFollowers { get; set; }
    }
}
