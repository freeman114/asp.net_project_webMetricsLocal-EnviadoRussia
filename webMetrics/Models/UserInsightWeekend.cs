using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webMetrics.Models
{
    public class UserInsightWeekend
    {
        public int Initial { get; set; }
        public int Final { get; set; }
        public decimal PercentInitial { get; set; }
        public decimal PercentFinal { get; set; }
    }
}
