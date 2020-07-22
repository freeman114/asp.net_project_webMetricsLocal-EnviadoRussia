
using MongoDB.Bson;
using System;

namespace webMetrics.Models
{
    public class AutorizacaoMetrica
    {
        public string UsuarioInstagram { get; set; }

        public string Email { get; set; }

        public string Key { get; set; }

        public string UsuarioId { get; set; }
        public string AgenciaUserId { get; set; }

        public DateTime DataCriacao { get; set; }

        public string Status { get; set; }


        public long Seguidores { get; set; }
        public double PowerFull { get; set; }
        public long Alcance { get; set; }
        public long Engajamento { get; set; }
        public int Aprovado { get; internal set; }
        public string ProfilePictureUrl { get; set; }

        public string Client { get; set; }

        public string _id { get; set; }
        public bool Reprocessar { get; set; }

        public bool LiberadoCredito { get; set; }
        public bool ProblemasToken { get; set; }
        public DateTime TimeSpan { get; set; }
    }
}