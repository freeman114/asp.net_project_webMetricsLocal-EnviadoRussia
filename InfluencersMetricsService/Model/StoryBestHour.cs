using System;

namespace InfluencersMetricsService.Model
{
    public class StoryBestHour
    {
        public string idStory { get; set; }
        public DateTime DateCreation { get; set; }
        public int Hour { get; set; }
        public int ValorReach { get; set; }
        public DayOfWeek DiaDaSemana { get; set; }
        public string UsuarioId { get; internal set; }
    }

    public class StoryUserBestHour
    {
        public string UsuarioId { get; set; }
        public int Hour { get; set; }
        public int ValorReach { get; set; }
        public DayOfWeek DiaDaSemana { get; set; }        
    }
}
