using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization;

namespace webMetrics.Models
{
    public class ContractClass<T>
    {
        public BsonObjectId _id { get; set; }
        
        public T Obj { get; set; }

        public string UsuarioInstagram { get; set; }
        public string UsuarioId { get; set; }
        public TimeSpan timeSpan { get; set; }

        public DateTime DateCreation { get; set; }
    }
}