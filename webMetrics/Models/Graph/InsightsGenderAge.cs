using System;
using System.Collections.Generic;

namespace webMetrics.Models.Graph
{
    public class InsightsGenderAge
    {
        public List<DatumI> data { get; set; }
    }

    public class InsigthDTO
    {
        public List<DatumDTO> data { get; set; }
    }

    public class ValueName
    {
        public string name { get; set; }
        public string valor { get; set; }
    }

    public class ValueDTO
    {
        public List<ValueName> value { get; set; }
        public string end_time { get; set; }
    }

    public class DatumDTO
    {
        public string name { get; set; }
        public string period { get; set; }
        public List<ValueDTO> values { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string id { get; set; }
    }

    public class Value
    {
        public Dictionary<string, int> value { get; set; }
        public string end_time { get; set; }
    }

    public class DatumI
    {
        public string name { get; set; }
        public string period { get; set; }
        //public Dictionary<string, int> values { get; set; }
        public List<Value> values { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string id { get; set; }
    }
}