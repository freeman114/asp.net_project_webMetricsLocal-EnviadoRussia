
using MongoDB.Bson;
using System;

namespace webMetrics.Models
{
    public class UsuarioAcoes
    {
        public string UsuarioId { get; set; }
        public string UserName { get; set; }
        public string Agencia { get; set; }
        public bool Usuario { get; set; }
        public bool Media { get; set; }
        public bool Tags { get; set; }
        public bool Stories { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Status { get; set; }
        public string CityTop { get; set; }
        public string AgeTop { get; set; }
    }
}