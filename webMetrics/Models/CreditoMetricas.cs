
using MongoDB.Bson;
using System;

namespace webMetrics.Models
{
    public class CreditoMetricas
    {
        public ObjectId _id { get; set; }

        public string UserId { get; set; }

        public int Qtd{ get; set; }
        public int? Debito { get; set; }

        public DateTime DataValidade { get; set; }

        public DateTime DataCredito { get; set; }

        public DateTime DataCriacao { get; set; }        
    }
}