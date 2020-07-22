using System;
using System.Collections.Generic;

namespace webMetrics.Models.DTO
{
    public class Story
    {
        public int ImpressionsValue { get; set; }
        public int ReachValue { get; set; }
        public int ExitsValue { get; set; }
        public int RepliesValue { get; set; }
        public int TapsForwardValue { get; set; }
        public int TapsBackValue { get; set; }
        public DateTime DateCreation { get; set; }
        public string Date { get; set; }
        public string Id { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public List<string> Imagens { get; set; }
    }
}