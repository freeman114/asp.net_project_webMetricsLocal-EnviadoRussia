using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webMetrics.Models
{
    public class InfluencerFollower
    {
        public string Username { get; set; }
        public int AvgComments { get; set; }
        public int Followers { get; set; }
        
        public bool Suspect { get; set; }
        public bool MassFollower { get; set; }
        public bool RealPerson { get; set; }
        public bool Influencer { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
