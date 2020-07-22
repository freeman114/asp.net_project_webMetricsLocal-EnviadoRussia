using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webMetrics.Models
{
    public class UsuarioCredito
    {
        public BsonObjectId _id { get; set; }
        public string NomeAgencia { get; set; }
        public string Telefone { get; set; }
        public string Nome { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Email { get; set; }
        public int Qtd { get; set; }
        public int? Debito { get; set; }
        public List<CreditoMetricas> Creditos { get; set; }
    };
}
