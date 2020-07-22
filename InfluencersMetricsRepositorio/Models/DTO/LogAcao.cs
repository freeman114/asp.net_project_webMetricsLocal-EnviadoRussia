using System;
using System.Collections.Generic;


namespace webMetrics.Models.DTO
{
    public class LogAcao
    {
        public LogAcao(string name, string local)
        {
            Name = name;
            Local = local;
        }

        public string Name { get; set; }
        public string Local { get; set; }
        public DateTime Data { get; set; }
    }    
}