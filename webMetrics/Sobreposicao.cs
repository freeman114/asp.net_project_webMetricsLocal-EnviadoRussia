using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webMetrics.Models.DTO;

namespace webMetrics
{
    public class Sobreposicao
    {
        private readonly IOptions<Models.AppSettings> _appSettings;
        public Sobreposicao(IOptions<Models.AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<List<SobrePosicaoResponse>> Get(string id, string UserId)
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _appSettings, UserId);
                var _id = new ObjectId(id);
                var lstSobrep = (await repMongo.ListarByOrderId<InfluencerComment>(_id)).ToList();

                if (lstSobrep != null)
                {
                    var lstSobrePosicaoResponse = new List<SobrePosicaoResponse>();
                    foreach (var sobrep in lstSobrep)
                    {
                        var sobrePosicaoResponse = new SobrePosicaoResponse();
                        sobrePosicaoResponse.Profile = sobrep.Obj.Influencer.Obj.profile_picture_url;
                        sobrePosicaoResponse.Username = sobrep.Obj.Influencer.Obj.username;
                        sobrePosicaoResponse.Before = sobrep.Obj.LstFollowersOld.Count();
                        sobrePosicaoResponse.After = sobrep.Obj.LstFollowers.Count();
                        lstSobrePosicaoResponse.Add(sobrePosicaoResponse);
                    }

                    return lstSobrePosicaoResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> ListMedia(List<string> lstUsuarioId, string UserId)
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _appSettings, UserId);
                var OrderId = ObjectId.GenerateNewId();

                var lstInfsUsuarios = new List<InfluencerComment>();
                foreach (var id in lstUsuarioId)
                {
                    var lstFollowers = new List<string>();
                    var _id = new ObjectId(id);
                    var lstMongoUser = await repMongo.ListarById<Models.Graph.Usuario>(_id);
                    if (lstMongoUser!=null && (lstMongoUser.Count>0))
                    {
                        var userId = lstMongoUser.FirstOrDefault().UsuarioId;
                        var lstMongoMedias = await repMongo.Listar<Models.Graph.Media>(userId);
                        if (lstMongoMedias != null && (lstMongoMedias.Count > 0))
                        {
                            lstMongoMedias.ForEach(f =>
                            {
                                if (f.Obj != null && (f.Obj.data != null && (f.Obj.data.Count > 0)))
                                {
                                    f.Obj.data.ForEach(d =>
                                    {
                                        if (d.comments != null && (d.comments.data != null && (d.comments.data.Count > 0)))
                                        {
                                            d.comments.data.ForEach(c =>
                                            {
                                                lstFollowers.Add(c.username);
                                            });
                                        }
                                    });
                                }
                            });
                        }
                        var inf = new InfluencerComment();
                        inf.OrderId = OrderId;
                        inf.DtCreated = DateTime.Now;
                        inf.LstFollowers = lstFollowers.Distinct().ToList();
                        inf.LstFollowersOld = lstFollowers.Distinct().ToList();
                        inf.Influencer = lstMongoUser.FirstOrDefault();
                        inf.UsuarioId = id;                        
                        lstInfsUsuarios.Add(inf);
                    }
                }

                foreach (var infSingle in lstInfsUsuarios)
                {
                    foreach (var infUsuario in lstInfsUsuarios)
                    {
                        if (infSingle.UsuarioId != infUsuario.UsuarioId)
                        {
                            infSingle.LstFollowers = infSingle.LstFollowersOld.Except(infUsuario.LstFollowersOld).ToList();
                        }
                    }
                    await repMongo.GravarOne<InfluencerComment>(infSingle);
                }

                return OrderId.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }

    public class SobrePosicaoResponse
    {
        public string Profile { get; set; }
        public string Username { get; set; }
        public int After { get; set; }
        public int Before { get; set; }

    }
}
