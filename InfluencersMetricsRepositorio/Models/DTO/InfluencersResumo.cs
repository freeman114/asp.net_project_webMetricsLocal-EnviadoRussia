using System.Collections.Generic;
using System.Linq;

namespace webMetrics.Models.DTO
{
    public class InfluencersResumo
    {
        public List<InstaMentions> LstUnrealComments;

        public double avgPostReach { get; set; }
        public double percentAvg { get; set; }

        public int Seguidores { get; set; }
        public int Seguindo { get; set; }
        public decimal SeguindoSeguidores { get; set; }
        public decimal SeguidoresUnicos { get; set; }
        public string ProfilePicture { get; set; }

        public int Posts { get; set; }
        public int Curtidas { get; set; }
        public int Comentarios { get; set; }
        public decimal MediaCurtidas { get; set; }
        public decimal MediaComentarios { get; set; }

        public decimal ComentariosSeguidores { get; set; }
        public decimal Engajamento { get; set; }
        public int Alcance { get; set; }
        public decimal MediaAlcancePost { get; set; }

        public List<InstaMentions> LstInstaMentions { get; set; }
        public List<InstaMentions> LstInstaMidias { get; set; }
        public List<InstaMentions> LstInstaHashs { get; set; }
        public string NomeCompleto { get; set; }
        public string UserName { get; set; }
        public string SocialContext { get; set; }
        public int Aprovado { get; set; }
        public List<InstaMentions> LstInstaMaps { get; set; }
        public List<InstaMentions> LstInstaPhotoTags { get; set; }
        public List<Models.FaceDetection> LstFaceDetection { get; set; }

        public Models.FaceDetection AvgFaceDetection { get; set; }

        public string Markers { get; set; }
        public string CabecalhoFaceDetection { get; set; }
        public string ListaFaceDetection { get; set; }
        public List<InstaMentions> LstDemographicRange { get; set; }
        public List<InstaMentions> LstAge { get; set; }
        public List<InstaMentions> LstBestMidiaPlacesRange { get; set; }
        public List<InstaMentions> LstTopAndBotton { get; set; }
    }

    public class InstaMentions
    {
        public string UserName { get; set; }
        public double Used { get; set; }
        public int Reach { get; set; }
        public double Engagemer { get; set; }
        public int DiffUsedEngag { get; set; }
        public List<string> Imagens { get; set; }
        public double UsedPerc { get; internal set; }
        public int Impressions { get; set; }
        public int Saveds { get; set; }
        public int Reachs { get; set; }
        public int Engagements { get; set; }
    }
}