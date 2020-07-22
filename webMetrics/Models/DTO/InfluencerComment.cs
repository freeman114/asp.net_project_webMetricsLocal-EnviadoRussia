using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webMetrics.Models.DTO
{
    public class InfluencerComment
    {
        public ObjectId OrderId { get; set; }
        public DateTime DtCreated { get; set; }
        public List<string> LstFollowers { get; set; }
        public List<string> LstFollowersOld { get; set; }
        public string UsuarioId { get; set; }
        public ContractClass<Graph.Usuario> Influencer { get; set; }
    }
}
