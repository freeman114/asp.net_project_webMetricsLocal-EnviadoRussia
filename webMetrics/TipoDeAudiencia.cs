using InfluencersMetricsService.Model;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using webMetrics.Models;
using webMetrics.Models.DTO;
using webMetrics.Models.Graph;

namespace webMetrics
{
    public class TipoDeAudiencia
    {
        private readonly IOptions<Models.AppSettings> _appSettings;

        public TipoDeAudiencia(IOptions<Models.AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<List<InfluencerFollower>> FindTipoAudiencia(string key, string metricasInsigths)
        {
            Repository.MongoRep repMongo = new Repository.MongoRep(metricasInsigths, _appSettings, key);

            var _userObj = await repMongo.ListarById<Models.Usuario>(new ObjectId(key));
            
            var lst = await ListUsers(key);
            var usersString = "";
            if (lst.Count > 0)
            {
                lst.ForEach(s => { usersString += "," + s.Username; });
            }

            var ot = await GetUsers(
                _userObj.FirstOrDefault().Obj.UsuarioInstagram,
                _userObj.FirstOrDefault().Obj.access_token_page,
                usersString);

            var infs = new List<InfluencerFollower>();
            foreach (var it in lst)
            {
                infs.Add(new InfluencerFollower()
                {
                    AvgComments = it.LengthComment,
                    DateCreated = DateTime.Now,
                    Followers = ot.Where(w => w.business_discovery.username == it.Username).FirstOrDefault().business_discovery.followers_count,
                    Username = it.Username,

                    MassFollower = (ot.Where(w => w.business_discovery.username == it.Username).FirstOrDefault().error == null) &&
                                    (it.LengthComment < 20),
                    Influencer = (ot.Where(w => w.business_discovery.username == it.Username).FirstOrDefault().error == null) &&
                                    (it.LengthComment >= 20),
                    Suspect = (ot.Where(w => w.business_discovery.username == it.Username).FirstOrDefault().error != null) &&
                                    (it.LengthComment < 20),
                    RealPerson = (ot.Where(w => w.business_discovery.username == it.Username).FirstOrDefault().error != null) &&
                                    (it.LengthComment >= 20),
                });
            }

            return infs;
        }

        public async Task<List<Models.Graph.Discovery>> GetUsers(string mediaId, string accesstoken, string usuarios)
        {
            try
            {
                var lst = new List<Models.Graph.Discovery>();
                foreach (var it in usuarios.Split(','))
                {
                    if (it.Length > 1)
                    {
                        var item = await GetDataGraphAsync<Models.Graph.Discovery>(accesstoken,
                           mediaId + "/?fields=business_discovery.username(" + it +
                           "){followers_count,media_count,media,name{comments_count,like_count,caption,media_url,timestamp}}");
                        
                        if (item.business_discovery == null)
                        {
                            item.business_discovery = new BusinessDiscoveryDisc()
                            {
                                username = it
                            };
                        }
                        else
                        {
                            item.business_discovery.username = it;
                        }

                        if (item != null)
                        {

                            lst.Add(item);

                        }
                        else
                        {
                            lst.Add(new Models.Graph.Discovery()
                            {
                                business_discovery = new BusinessDiscoveryDisc()
                                {
                                    username = it
                                }
                            });
                        }
                        //if (item.error != null && (item.error.code == 190))
                        //{

                        //    break;
                        //}
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<FollowerComments>> ListUsers(string UserId)
        {
            Repository.MongoRep repMongo = new Repository.MongoRep("Metricas Insigths", _appSettings, UserId);
            var lstFollowers = new List<FollowerComments>();
            var lstMongoMedias = await repMongo.Listar<Models.Graph.Media>(UserId);
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
                                    lstFollowers.Add(new FollowerComments() {
                                        Username=    c.username,
                                        LengthComment = c.text.Length
                                    });
                                });
                            }
                        });
                    }
                });
            }

            return lstFollowers.GroupBy(g => new { g.Username })
                .Select( s=> new FollowerComments()
                {
                    Username = s.Key.Username,
                    LengthComment = Convert.ToInt32((lstFollowers.Where(w=>w.Username==s.Key.Username).Average(l=>l.LengthComment)))
                })
                .Distinct().ToList();
        }

        HttpClient client = new HttpClient();
        public async Task<T> GetDataGraphAsync<T>(string accessToken, string uri)
        {
            var conc = uri.Contains("?") ? "&" : "?";
            var _urlFull = $"https://graph.facebook.com/v3.2/{uri}{conc}access_token={accessToken}";
            try
            {
                var response = await client.GetAsync(_urlFull);

                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(result);
                }

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception)
            {

                return default(T);
            }
        }
    }

    public class TipoDeAudienciaResponse
    {
        public string UsuarioId { get; set; }
        public List<InfluencerFollower> Seguidores { get; set; }
        public string TypeAudience { get; set; }
    }
}
