using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluencersMetricsService.Model
{
    public class Comment
    {
        public DataValue[] data { get; set; }
        public DateTime DtCreated { get; set; }
        public string IdUser { get; set; }
    }
    
    public class DataValue
    {        
      public string text { get; set; }
      public DateTime timestamp { get; set; }
      public string username { get; set; }
      public string id { get; set; }    
    }
}
