namespace webMetrics.Models
{
    public class Metricas
    {
        public Cabecalho Cabecalho { get; set; }
        public Sumario Sumario { get; set; }

        public Models.DTO.InfluencersResumoFree inf { get; set; }
    }

    public class Cabecalho
    {
        public double Seguidores { get; set; }
        public double Seguindo { get; set; }
        public double Mediaengajamento { get; set; }
        public double Mediaalcance { get; set; }
        public double Viewperfil { get; set; }
        public double Reach { get; set; }
        public double Mediaimpressoes { get; set; }
        public double Audienciacrescimento { get; set; }
    }

    public class Sumario
    {
        public double Mediastories { get; set; }
        public double Videosstories { get; set; }
        public double Fotosstories { get; set; }
        public double Posts { get; set; }
        public double Avgpostsdiario { get; set; }
        public double Avgpostssemanal { get; set; }
        public double Avgpostsmensal { get; set; }
        public double Totallikes { get; set; }
        public double Medialikes { get; set; }
        public double Totalcomentarios { get; set; }
        public double Mediacomentarios { get; set; }
        public double Seguindoseguidores { get; set; }
        public double Comentariosseguidores { get; set; }
        public double Videospostfeed { get; set; }
        public double Fotospostfeed { get; set; }
    }
}