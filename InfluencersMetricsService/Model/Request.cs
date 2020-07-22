using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluencersMetricsService.Model
{
    public class Request<T>
    {
        public T Obj { get; set; }
        public string userId { get; set; }
        public string usuarioinstagram { get; set; }
    }
}
