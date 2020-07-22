using InstaSharper.Classes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace webMetrics.Models
{
    public class InstaMediaHash 
    {
        public InstaMedia InstaMedia { get; set; }
        public List<string> Hashs { get; set; }
        public int Impressions { get; set; }
        public int Saveds { get; set; }
        public int Engagement { get; set; }
        public int Reachs { get; set; }
    }
}