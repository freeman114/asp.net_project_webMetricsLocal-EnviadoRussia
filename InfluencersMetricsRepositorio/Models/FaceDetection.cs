using System;

namespace webMetrics.Models
{
    public class FaceDetection
    {
        public string UserName { get; set; }
        public string UrlImagem { get; set; }
        public int Joy { get; set; }
        public int Anger { get; set; }
        public int Sorrow { get; set; }
        public int Surprise { get; set; }
        public DateTime DtAvaliacao { get; set; }

        public string MediaId { get; set; }
    }
}