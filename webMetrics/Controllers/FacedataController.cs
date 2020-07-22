using InfluencersMetricsService.Helper;
using InfluencersMetricsService.Model;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using webMetrics.Models;

namespace webMetrics.Controllers
{
    //    [EnableCors((origins: "*", headers: "*", methods: "*")]
    public class FacedataController : Controller
    {
        private readonly IOptions<Models.AppSettings> _appSettings;

        public FacedataController(IOptions<Models.AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public ActionResult Index(string key)
        {
            ViewBag.key = key;
            return View();
        }

        HttpClient client = new HttpClient();

        [HttpPost]
        public async Task<ActionResult<string>> LoadEmotionProfile([FromBody]
            InfluencersMetricsService.Model.Request<List<string>> request)
        {
            try
            {
                var repMongo = new Repository.MongoRep("", _appSettings, request.userId);
                var lstFaceDetection = new List<Models.FaceDetection>();
                foreach (var it in request.Obj)
                {
                    var facedetection = await getAnnotation(it);
                    if (facedetection != null) lstFaceDetection.Add(facedetection);
                }

                if (lstFaceDetection.Count > 0)
                {
                    await repMongo.GravarOne<List<Models.FaceDetection>>(lstFaceDetection);
                }

                return "";
            }
            catch (Exception ex)
            {
                return "#error:" + ex.Message.ToString();
            }
        }

        [HttpPost]
        public async Task<ActionResult<bool>> setEmotionalDiscovery([FromBody]
            string _id)
        {
            Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings, _id);

            var ret = await repMongo.SetMediaDoscoveryEmotional(_id);
            return false;
        }

        [HttpPost]
        public async Task<ActionResult<List<MediaToken>>> ListarMediasWithEmotionalByAgenciaAsync([FromBody]
            InfluencersMetricsService.Model.Request<List<string>> request)
        {
            Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings, request.userId);

            var lstMedias = await repMongo.ListarMediasWithEmotionalByAgencia<string>();
            var lstUserIdsAgencias = lstMedias.Select(s => s.UsuarioId).Distinct();

            var tokens = await repMongo.ListarTokensUserByEmail<Models.Usuario>(lstUserIdsAgencias.ToList());
            var lstMediasWithTokens = lstMedias.Select(s => new MediaToken
            {
                data = s.Obj.business_discovery.media.data,
                IdDiscovery = s._id.ToString(),
                Token = tokens.Where(w => w.Obj.Email == s.UsuarioId).FirstOrDefault().Obj.access_token_page,
                Account_Business_Instagram = s.Obj.id
            });

            return lstMediasWithTokens.ToList();
        }

        [HttpPost]
        public async Task<ActionResult<string>> setToken(string instagram_business_account,
                string access_token, string nomePage)
        {
            if (string.IsNullOrEmpty(instagram_business_account))
            {
                return "Error#Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(access_token))
            {
                return "Error#Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(nomePage))
            {
                return "Error#Não recebemos o valor dos insigths.";
            }

            client = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/v3.2/")
            };
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await GetTokenLongAsync<Models.AccessTokenResponse>(access_token);

            var _id = HttpContext.Session.GetString("UsuarioFull_id");
            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);
            if (string.IsNullOrEmpty(UserId))
            {
                return "Error#Usuario não esta logado.";
            }
            else
            {
                await repMongo.AlterarToken(new Models.ContractClass<Models.Usuario>()
                {
                    _id = new MongoDB.Bson.BsonObjectId(new ObjectId(_id)),
                    Obj = new Models.Usuario()
                    {
                        access_token_page = result.access_token,
                        name_page = nomePage,
                        UsuarioInstagram = instagram_business_account
                    }
                });
                HttpContext.Session.SetString("access_token_page", result.access_token);
                HttpContext.Session.SetString("name_page", nomePage);
                HttpContext.Session.SetString("instagram_business_account", instagram_business_account ?? "");
            }
            return "";
        }

        public async Task<ActionResult> ViewData(string accesstoken)
        {
            client = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com/v3.2/")
            };
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = await GetTokenLongAsync<Models.AccessTokenResponse>(accesstoken);

            var _id = HttpContext.Session.GetString("UsuarioFull_id");
            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);
            if (string.IsNullOrEmpty(UserId))
            {
                return Ok("Error#Usuario não esta logado.");
            }
            else
            {
                string nomePage = "";
                await repMongo.AlterarToken(new Models.ContractClass<Models.Usuario>()
                {
                    _id = new MongoDB.Bson.BsonObjectId(new ObjectId(_id)),
                    Obj = new Models.Usuario()
                    {
                        access_token_page = result.access_token,
                        name_page = nomePage
                    }
                });
                HttpContext.Session.SetString("access_token_page", result.access_token);
                HttpContext.Session.SetString("name_page", nomePage);
            }
            return Ok("");
        }

        [HttpPost]
        public async Task<bool> SetMediaInsight([FromBody]_request req)
        {
            Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);
            return await repMongo.SetMediaInsight(req.userId, req.mediaId, req.impressions, req.reach, req.saved, req.engagement, req.mediaGraphId);

        }

        [HttpPost]
        public async Task<ActionResult<string>> setTokenUsuario(string instagram_business_account, string access_token, string nomePage, string userId, string agenciaUserId, string clienteId = "")
        {
            if (string.IsNullOrEmpty(instagram_business_account))
            {
                return "Error#Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(access_token))
            {
                return "Error#Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(nomePage))
            {
                return "Error#Não recebemos o valor dos insigths.";
            }

            try
            {
                client = new HttpClient
                {
                    BaseAddress = new Uri("https://graph.facebook.com/v3.2/")
                };
                client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Repository.MongoRep repMongo = new Repository.MongoRep("MetricaInsights", _appSettings);

                //Verifico se já existe pra agencia e pro usuario_account_instagram_business
                var exist = await repMongo.ListUserIdByAgencia(agenciaUserId, instagram_business_account, nomePage);
                var _idUsuarioExistente = "";
                //if (exist)
                //{
                //    _idUsuarioExistente = await repMongo.ConsultaBloqueioProblemasToken(agenciaUserId, nomePage, instagram_business_account);
                //    if (!string.IsNullOrEmpty(_idUsuarioExistente))
                //    {
                //        exist = false;
                //    }
                //}

                if (!exist)
                {
                    var result = await GetTokenLongAsync<Models.AccessTokenResponse>(access_token);
                    var usuarioNovo = new Models.Usuario()
                    {
                        access_token_page = result.access_token,
                        name_page = nomePage,
                        UsuarioInstagram = instagram_business_account,
                        UserId = userId,
                        Tipo = "4",
                        AgenciaUserId = agenciaUserId,
                        NomeAgencia = clienteId == "" ? null : clienteId
                    };
                    if (string.IsNullOrEmpty(_idUsuarioExistente))
                    {
                        await repMongo.GravarOne<Models.Usuario>(usuarioNovo);
                    }
                    else
                    {
                        await repMongo.AlterarUsuarioToken(new ContractClass<Usuario>()
                        {
                            Obj = usuarioNovo,
                            _id = new ObjectId(_idUsuarioExistente)
                        });

                        //Remover bloqueio caso exista

                    }

                    var listUsuario = await repMongo.ListByInstagramBusinessAccount<Models.Usuario>(instagram_business_account);
                    var id = listUsuario.OrderByDescending(o => o.timeSpan).FirstOrDefault()._id;

                    return id.ToString();
                }
                else
                {
                    return "Error#Influencer tem cadastro na agência, como: instagram_business_account(" + instagram_business_account + ") e name_page(" + nomePage + ")";
                }
            }
            catch (Exception ex)
            {
                return "Error#" + ex.Message.ToString();
            }
        }

        public async Task<T> GetTokenLongAsync<T>(string accessToken)
        {
            string appid = "220440968764019";
            string appsecret = "4ac0249655cc5c39205ac54a77d467ce";
            var response = await client
                .GetAsync($"/oauth/access_token?grant_type=fb_exchange_token&client_id={appid}&client_secret={appsecret}&fb_exchange_token={accessToken}");
            if (!response.IsSuccessStatusCode)
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("MetricaInsights", _appSettings);
                await repMongo.GravarOne<HttpResponseMessage>(response);
                return default(T);
            }

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }

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
                    var erros = JsonConvert.DeserializeObject<Models.Graph.ResponseGraph>(result);

                    //Verificar se o erro suspende temporario ou definitivamente o processo
                    if (erros != null && (erros.error != null))
                    {
                        if (erros.error.code == 32) //4800 chamadas/pessoa/24 horas
                        {
                            //Scheduler.access_token_suspensos.Add(new Model.Suspensao()
                            //{
                            //    AccessToken = accessToken,
                            //    DtExpirou = DateTime.Now.AddHours(6)
                            //}
                            //);
                        }

                        if (erros.error.code == 100) //Media com erro
                        {
                            //Scheduler.media_error.Add(uri);
                        }

                        if (erros.error.code == 190) //Tokn Expirou
                        {
                            //Scheduler.media_error.Add(uri);
                        }

                    }

                    return default(T);
                }


                //WriteErrorLog("Result:" + result.ToString());

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception)
            {

                return default(T);
            }
        }

        [HttpGet]
        public async Task<List<InfluencersMetricsService.Model.Response<Models.Graph.Media>>> GetListMediaInsightsPendingAsync(int dias = 60)
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);
                var lstMedias = await repMongo.GetMediaWithoutInsight(dias);
                var lstMediasWithUser = lstMedias.Where(w => w.UsuarioId != null);

                ObjectId testandoBson;
                var lstUsuarios = await repMongo.ListarUsuarioByAgencia<Models.Usuario>(
                        lstMediasWithUser.Where(w => ObjectId.TryParse(w.UsuarioId, out testandoBson))
                        .Select(s =>
                            new BsonObjectId(new ObjectId(s.UsuarioId))
                        ).ToList()
                    );
                var lstUsuariosNoAgencia = await repMongo.ListarTokensUserByUserId(
                        lstMediasWithUser.Where(w => ObjectId.TryParse(w.UsuarioId, out testandoBson) == false)
                        .Select(s =>
                            s.UsuarioId
                        ).ToList()
                    );

                var lstResponseInfluencers = lstUsuarios.Select(s => new InfluencersMetricsService.Model.Response<Models.Graph.Media>()
                {
                    Obj = lstMediasWithUser.Where(m => m.UsuarioId == s._id.ToString()).FirstOrDefault().Obj,
                    access_token = s.Obj.access_token_page,
                    instagram_business_account = s.Obj.UsuarioInstagram,
                    userId = s._id.ToString(),
                    id = lstMediasWithUser.Where(m => m.UsuarioId == s._id.ToString()).FirstOrDefault()._id.ToString()

                }).ToList();

                var lstResponseNoAgencia = lstUsuariosNoAgencia.Select(s => new InfluencersMetricsService.Model.Response<Models.Graph.Media>()
                {
                    Obj = lstMediasWithUser.Where(m => m.UsuarioId == s.Obj.UserId.ToString()).FirstOrDefault().Obj,
                    access_token = s.Obj.access_token_page,
                    instagram_business_account = s.Obj.UsuarioInstagram,
                    userId = s._id.ToString(),
                    id = lstMediasWithUser.Where(m => m.UsuarioId == s.Obj.UserId.ToString()).FirstOrDefault()._id.ToString()
                }).ToList();

                var lstResponse = lstResponseInfluencers.Union(lstResponseNoAgencia).ToList().Where(w => w.userId != "").ToList();
                return lstResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<List<InfluencersMetricsService.Model.Response<Models.Graph.Media>>> GetListMediaInsightsAsync()
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);
                var lstMedias = await repMongo.GetListMediaInsightsAsync();
                var lstMediasWithUser = lstMedias.Where(w => w.UsuarioId != null);

                var lstTokens = await repMongo.ListarPendingTokens();
                var UserIds = lstTokens.Select(s =>
                    (s.Obj.Tipo == "1") ? s.Obj.UserId.ToString() : s._id.ToString()
                ).ToList();

                return lstMediasWithUser.Select(s => new InfluencersMetricsService.Model.Response<Models.Graph.Media>()
                {
                    Obj = s.Obj,
                    access_token = lstTokens
                            .Where(ac => ((ac.Obj.Tipo == "1") ? ac.Obj.UserId.ToString() : ac._id.ToString()) == s.UsuarioId)
                            .FirstOrDefault().Obj.access_token_page,
                    instagram_business_account = lstTokens
                            .Where(ac => ((ac.Obj.Tipo == "1") ? ac.Obj.UserId.ToString() : ac._id.ToString()) == s.UsuarioId)
                            .FirstOrDefault().Obj.UsuarioInstagram,
                    userId = s.UsuarioId
                }).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }
        [HttpPost]
        public async Task<string> SetCorrecoes([FromBody]_reqJsonData req)
        {
            var result = await setJsonData(req.json, req.namePage, req.key, req.nameData);
            return result.Value;
        }

        //[HttpPost]
        public async Task<Models.CreditoMetricas> GetCredito(string UserId, Repository.MongoRep repMongo = null)
        {
            if (repMongo == null)
            {
                repMongo = new Repository.MongoRep("", _appSettings);
            }
            var list = await repMongo.ListarCreditos(UserId);
            if (list == null || (list.Count == 0)) return null;

            list = list.Where(w =>  //TODO: debitos na conta...
                (w.Obj.DataValidade >= DateTime.Now)).ToList();

            if (list == null || (list.Count == 0)) return null;

            var cred = new Models.CreditoMetricas()
            {
                DataCredito = list.Max(m => m.Obj.DataCredito),
                DataCriacao = DateTime.Now,
                DataValidade = list.Max(m => m.Obj.DataValidade),
                Qtd = list.Sum(m => m.Obj.Qtd),
                Debito = list.Sum(m => m.Obj.Debito),
                UserId = UserId
            };
            return cred;
        }

        public async Task<Models.CreditoMetricas> Debitar(string UserId, int qtd, Repository.MongoRep repMongo = null)
        {
            if (repMongo == null)
            {
                repMongo = new Repository.MongoRep("", _appSettings);
            }
            var list = await repMongo.ListarCreditos(UserId);
            if (list.Count() > 0)
            {
                var lstCreditos = list.Where(x => x.Obj.Qtd != x.Obj.Debito);
                var item = lstCreditos
                    .Where(w => w.Obj.DataValidade == lstCreditos.Min(m => m.Obj.DataValidade)).FirstOrDefault();
                if (item != null)
                {
                    item.Obj.Debito = item.Obj.Debito + 1;
                    await repMongo.AlterarCredito(item);
                    return item.Obj;
                }
                else
                {
                    //sem saldo
                    var cred = new Models.CreditoMetricas()
                    {
                        DataCredito = DateTime.Now,
                        DataCriacao = DateTime.Now,
                        DataValidade = DateTime.Now,
                        Qtd = 0,
                        Debito = 0,
                        UserId = UserId
                    };
                    return cred;
                }
            }
            else
            {
                var cred = new Models.CreditoMetricas()
                {
                    DataCredito = DateTime.Now,
                    DataCriacao = DateTime.Now,
                    DataValidade = DateTime.Now,
                    Qtd = 0,
                    Debito = 0,
                    UserId = UserId
                };
                return cred;
            }
        }


        public async Task<ActionResult> novaconsulta(string id, string origem)
        {
            if (origem == "c")
            {
                #region Consultas a serem reprocessadas
                var UserId = HttpContext.Session.GetString("UsuarioFull_id");

                Repository.MongoRep repMongoU = new Repository.MongoRep(UserId, _appSettings, UserId);
                var usuarioGraph = await repMongoU.ListarById<Models.Graph.Usuario>(new ObjectId(id));
                if (usuarioGraph == null)
                {
                    return null;
                }
                var usuario = await repMongoU.ListarById<Models.Usuario>(new ObjectId(usuarioGraph.FirstOrDefault().UsuarioId));

                var access_token = usuario.FirstOrDefault().Obj.access_token_page;
                var _instagram_business_account = usuario.FirstOrDefault().Obj.UsuarioInstagram;

                var _dataUsoCredito = usuario.FirstOrDefault().Obj.DataUsoCredito;
                if (usuario.FirstOrDefault().Obj.Tipo != "1")
                {
                    if (usuario.FirstOrDefault().Obj.DataUsoCredito < DateTime.Now.AddDays(-31))
                    {
                        var _UserId = HttpContext.Session.GetString("UserId");
                        var _credito = await GetCredito(_UserId, repMongoU);
                        var saldo = 0;
                        if (_credito != null)
                        {
                            saldo = Convert.ToInt32(_credito.Qtd - _credito.Debito);
                        }
                        if (saldo == 0)
                        {
                            return RedirectToAction("historicometricas", "relatorios", new { msg = "6" });
                        }
                        _dataUsoCredito = DateTime.Now;
                        await Debitar(_UserId, 1, repMongoU);
                    }
                }

                await repMongoU.GravarOne<Models.Usuario>(
                new Models.Usuario()
                {
                    access_token_page = access_token,
                    name_page = usuario.FirstOrDefault().Obj.name_page,
                    UsuarioInstagram = _instagram_business_account,
                    UserId = usuario.FirstOrDefault().Obj.Tipo == "1" ? UserId : null,
                    Tipo = usuario.FirstOrDefault().Obj.Tipo,
                    AgenciaUserId = usuario.FirstOrDefault().Obj.AgenciaUserId,
                    DataUsoCredito = _dataUsoCredito,
                    StatusCredito = usuario.FirstOrDefault().Obj.StatusCredito
                });

                var listUsuario = await repMongoU.ListByInstagramBusinessAccount<Models.Usuario>(_instagram_business_account);

                var _id = listUsuario.OrderByDescending(o => o.timeSpan).FirstOrDefault()._id.ToString();

                Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, _id);
                var resultUsuario = await GetDataGraphAsync<Models.Graph.Usuario>(access_token, _instagram_business_account +
                    "?fields=biography,id,ig_id,followers_count,follows_count,media_count,name,profile_picture_url,username,website");
                var resultUsuarioData = await setJsonData(JsonConvert.SerializeObject(resultUsuario), "novaconsulta", _id, "Usuario");

                var resultMedia = await GetDataGraphAsync<Models.Graph.Media>(access_token, _instagram_business_account +
                    "/media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username");
                var resultMediaData = await setJsonData(JsonConvert.SerializeObject(resultMedia), "novaconsulta", _id, "Media");

                #region Tipo de audiencia
                var ta = new TipoDeAudiencia(_appSettings);
                var objLstTipoAudiencia = await ta.FindTipoAudiencia(UserId, UserId);
                await repMongo.GravarOne(objLstTipoAudiencia);
                #endregion

                var resultTags = await GetDataGraphAsync<Models.Graph.Tags>(access_token, _instagram_business_account +
                    "/tags?fields=caption,owner,username,media_url,comments_count,like_count&limit=25");
                var resultTagsData = await setJsonData(JsonConvert.SerializeObject(resultTags), "novaconsulta", _id, "tags");

                var resultInsightsAge = await GetDataGraphAsync<Models.DTO.Insigth>(access_token, _instagram_business_account +
                    "/insights?metric=audience_gender_age&period=lifetime");
                var resultInsightsAgeData = await setJsonData(JsonConvert.SerializeObject(resultInsightsAge), "novaconsulta", _id, "InsightsGenderAge");

                var resultInsightsCity = await GetDataGraphAsync<Models.Graph.InsightsGenderAge>(access_token, _instagram_business_account +
                    "/insights?metric=audience_city&period=lifetime");
                var resultInsightsCityData = await setJsonData(JsonConvert.SerializeObject(resultInsightsCity), "novaconsulta", _id, "InsightsCity");

                var resultInsightsUserPro = await GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>(access_token, _instagram_business_account +
                    "/insights?metric=profile_views&period=day");
                var resultInsightsUserProData = await setJsonData(JsonConvert.SerializeObject(resultInsightsUserPro), "novaconsulta", _id, "UserInsights");

                var resultInsightsUser = await GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>(access_token, _instagram_business_account +
                    "/insights?metric=impressions,reach&period=week");
                var resultInsightsUserData = await setJsonData(JsonConvert.SerializeObject(resultInsightsUser), "novaconsulta", _id, "UserInsights");

                await SetStoryInsight(_instagram_business_account, _id, access_token);
                var resultStory = await GetDataGraphAsync<Models.Graph.Stories>(access_token, _instagram_business_account +
                    "/stories?fields=media_url,permalink,username,owner,media_type,shortcode,timestamp,caption");
                var resultStoryData = await setJsonData(JsonConvert.SerializeObject(resultStory), "novaconsulta", _id, "Stories");



                var result = await repMongo.AlterarProcessamentoUsuario(new ObjectId(id));

                return RedirectToAction("historicometricas", "relatorios");
                #endregion
            }
            else if (origem == "m")
            {
                #region Novas consultas em Minhas analises
                try
                {
                    var _id = HttpContext.Session.GetString("UsuarioFull_id");

                    Repository.MongoRep repMongoU = new Repository.MongoRep(_id, _appSettings, _id);
                    var usuario = await repMongoU.ListarById<Models.Usuario>(new ObjectId(id));
                    if (usuario == null)
                    {
                        return RedirectToAction("minhasanalises", "relatorios", new { msg = "99" });
                    }

                    var access_token = usuario.FirstOrDefault().Obj.access_token_page;
                    var _instagram_business_account = usuario.FirstOrDefault().Obj.UsuarioInstagram;

                    var _UserId = HttpContext.Session.GetString("UserId");

                    Repository.MongoRep repMongo = new Repository.MongoRep(_UserId, _appSettings, _id);
                    var resultUsuario = await GetDataGraphAsync<Models.Graph.Usuario>(access_token, _instagram_business_account +
                        "?fields=biography,id,ig_id,followers_count,follows_count,media_count,name,profile_picture_url,username,website");
                    var resultUsuarioData = await setJsonData(JsonConvert.SerializeObject(resultUsuario), "novaconsulta", _id, "Usuario");

                    var resultMedia = await GetDataGraphAsync<Models.Graph.Media>(access_token, _instagram_business_account +
                        "/media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username");
                    var resultMediaData = await setJsonData(JsonConvert.SerializeObject(resultMedia), "novaconsulta", _id, "Media");

                    var resultTags = await GetDataGraphAsync<Models.Graph.Tags>(access_token, _instagram_business_account +
                        "/tags?fields=caption,owner,username,media_url,comments_count,like_count&limit=25");
                    var resultTagsData = await setJsonData(JsonConvert.SerializeObject(resultTags), "novaconsulta", _id, "tags");

                    var resultInsightsAge = await GetDataGraphAsync<Models.DTO.Insigth>(access_token, _instagram_business_account +
                        "/insights?metric=audience_gender_age&period=lifetime");
                    var resultInsightsAgeData = await setJsonData(JsonConvert.SerializeObject(resultInsightsAge), "novaconsulta", _id, "InsightsGenderAge");

                    var resultInsightsCity = await GetDataGraphAsync<Models.Graph.InsightsGenderAge>(access_token, _instagram_business_account +
                        "/insights?metric=audience_city&period=lifetime");
                    var resultInsightsCityData = await setJsonData(JsonConvert.SerializeObject(resultInsightsCity), "novaconsulta", _id, "InsightsCity");

                    var resultInsightsUserPro = await GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>(access_token, _instagram_business_account +
                        "/insights?metric=profile_views&period=day");
                    var resultInsightsUserProData = await setJsonData(JsonConvert.SerializeObject(resultInsightsUserPro), "novaconsulta", _id, "UserInsights");

                    var resultInsightsUser = await GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>(access_token, _instagram_business_account +
                        "/insights?metric=impressions,reach&period=week");
                    var resultInsightsUserData = await setJsonData(JsonConvert.SerializeObject(resultInsightsUser), "novaconsulta", _id, "UserInsights");

                    var resultStory = await GetDataGraphAsync<Models.Graph.Stories>(access_token, _instagram_business_account +
                        "/stories?fields=media_url,permalink,username,owner,media_type,shortcode,timestamp,caption");
                    var resultStoryData = await setJsonData(JsonConvert.SerializeObject(resultStory), "novaconsulta", _id, "Stories");

                    return Ok("");
                }
                catch (Exception ex)
                {
                    return Ok("Error#Exceptions ==> " + ex.Message);
                }
                #endregion
            }
            else
            {
                return RedirectToAction("login", "relatorios");
            }
        }

        [HttpGet]
        public async Task<List<string>> GetListPaymentsPendentAsync()
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);
                var lstPays = await repMongo.ListarPendingPay();
                var result = lstPays.Select(s => s._id.ToString()).ToList();

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<List<InfluencersMetricsService.Model.Response<string>>> GetUsersTokensAsync()
        {
            Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);

            var lstTokens = repMongo.ListarPendingTokens().Result;
            var UserIds = lstTokens.Select(s => s._id.ToString()).ToList();

            var lstUsersGraphs = await repMongo.ListarGraphIdByAgencia<Models.Graph.Usuario>(UserIds);
            var newListInfluencer = lstTokens
                .Select(s => new InfluencersMetricsService.Model.Response<string>()
                {
                    access_token = s.Obj.access_token_page,
                    Obj = null,
                    userId = s._id.ToString()
                }).ToList();

            newListInfluencer.ForEach(f =>
            {
                var obj = lstUsersGraphs.Where(w => w.UsuarioId == f.userId).ToList();
                foreach (var it in obj)
                {
                    f.Obj = (it.Obj != null) ? it.Obj.id : f.Obj;
                }
            });
            return newListInfluencer.Where(w => w.Obj != null).ToList();
        }

        [HttpGet]
        public async Task<List<InfluencersMetricsService.Model.Response<string>>> GetInsightUserAsync()
        {
            Repository.MongoRep repMongo = new Repository.MongoRep("", _appSettings);

            #region Listando Tokens
            var lstTokens = repMongo.ListarPendingTokens().Result;

            var Ids = lstTokens.Select(s => s._id.ToString()).ToList();

            var lstUsersGraphs = await repMongo.ListGraphByUserIds<Models.Graph.Usuario>(Ids);

            var newList = lstTokens.Select(s => new InfluencersMetricsService.Model.Response<string>()
            {
                access_token = s.Obj.access_token_page,
                instagram_business_account = s.Obj.UsuarioInstagram,
                userId = s._id.ToString(),
                Obj = lstUsersGraphs.Where(f => f.UsuarioId == s._id.ToString()).Count() > 0 ?
                        lstUsersGraphs.Where(f => f.UsuarioId == s._id.ToString()).FirstOrDefault().Obj.id
                        :
                        null
            });
            var lstTokensOficial = newList.Where(w => w.Obj != null).ToList();
            #endregion

            var lst = await repMongo.ListarInsightsUsers<InfluencersMetricsService.Model.UserInsights>(Ids);

            var lstPendentes = new List<string>();
            if (lst != null && (lst.Count() > 0))
            {
                lstPendentes = lstTokensOficial.Where(w => !lst.Select(s => s.UsuarioId).Contains(w.userId))
                    .Select(se => se.userId).ToList();
            }
            else
            {
                lstPendentes = lstTokensOficial.Select(se => se.userId).ToList();
            }

            var result = lstTokensOficial.Where(w => lstPendentes.Contains(w.userId)).Select(s => new InfluencersMetricsService.Model.Response<string>()
            {
                Obj = s.Obj,
                instagram_business_account = s.instagram_business_account,
                access_token = s.access_token,
                userId = s.userId
            }).ToList();

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<string>> loadJsonData(string json, string usuario, string key, string nameData)
        {
            if (string.IsNullOrEmpty(json))
            {
                return "Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(usuario) && string.IsNullOrEmpty(key))
            {
                return "usuario e chave não encontrados.";
            }

            Repository.MongoRep repMongo = new Repository.MongoRep(usuario, _appSettings);
            if (!string.IsNullOrEmpty(key))
            {
                var lstMetricas = await repMongo.GetMetrica(usuario, key);
                if (lstMetricas == null || (lstMetricas.Count == 0))
                {
                    return "Autorização de métrica não encontrada.";
                }
                else
                {
                    var autorizacao = lstMetricas.FirstOrDefault();
                    if (autorizacao.Obj.Status == null || (autorizacao.Obj.Status != EnumStatus.PROCESSADO))
                    {
                        usuario = autorizacao.UsuarioInstagram;
                    }
                    autorizacao.Obj.Status = EnumStatus.PROCESSADO;
                    await repMongo.AlterarMetrica(autorizacao);
                }
            }

            await repMongo.GravarOne<string>(json);
            return "usuario:'" + usuario + "';" + nameData;
        }

        [HttpPost]
        public async Task<ActionResult<List<SobrePosicaoResponse>>> GetSobreposicaoDetail(string id, string userid)
        {
            var sob = new Sobreposicao(_appSettings);
            var resultado = await sob.Get(id, userid);

            return resultado;
        }

        [HttpPost]
        public async Task<ActionResult<string>> GetSobreposicao(string userid, string listUsers)
        {//"5d12d02f8daa4f08e87e85ae", "5cf6c8efb869bc2a84fa80a4"
            //"5c5256bb8344931f68f2e93f"
            var sob = new Sobreposicao(_appSettings);
            var lst = listUsers.Split(',').ToList<string>().Where(x=>!string.IsNullOrEmpty(x)).OrderBy(o=>o).ToList<string>();

            var resultado = await sob.ListMedia(lst, userid);

            return resultado;
        }

        [HttpPost]
        public async Task<ActionResult<string>> setJsonData(string json, string namePage, string key, string nameData)
        {
            if (string.IsNullOrEmpty(json))
            {
                return "Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(namePage) && string.IsNullOrEmpty(key))
            {
                return "usuario e chave não encontrados.";
            }

            try
            {

                Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, key);
                if (nameData == "Usuario")
                {   
                    var js = JsonConvert.DeserializeObject<Models.Graph.Usuario>(json);
                    #region Gravar Imagem profile na Amazon
                    WebClient webClient = new WebClient();
                    var idUnique = Guid.NewGuid().ToString();
                    var remoteFileUrl = js.profile_picture_url;
                    var _urlLocal = @"c:\output\" + idUnique + ".jpg";
                    webClient.DownloadFile(remoteFileUrl, _urlLocal);
                    var resultName = UploadFile.UploadFileLoad(_urlLocal, idUnique + ".jpg");
                    js.profile_picture_url = resultName;                    
                    #endregion
                    await repMongo.GravarOne<Models.Graph.Usuario>(js);
                }
                if (nameData == "Media")
                {
                    var js = JsonConvert.DeserializeObject<Models.Graph.Media>(json);
                    await repMongo.GravarOne(js);

                    #region Tipo de audiencia
                    var ta = new TipoDeAudiencia(_appSettings);
                    var objLstTipoAudiencia = await ta.FindTipoAudiencia(key, "MetricasInsights");
                    await repMongo.GravarOne(objLstTipoAudiencia);
                    #endregion
                }
                if (nameData == "tags")
                {
                    var js = JsonConvert.DeserializeObject<Models.Graph.Tags>(json);
                    await repMongo.GravarOne<Models.Graph.Tags>(js);
                }
                if (nameData == "Stories")
                {
                    var js = JsonConvert.DeserializeObject<Models.Graph.Stories>(json);
                    await repMongo.GravarOne<Models.Graph.Stories>(js);
                }
                if (nameData == "dataStories")
                {
                    var js = JsonConvert.DeserializeObject<Models.DTO.InsigthStory>(json);
                    await repMongo.GravarOne<Models.DTO.InsigthStory>(js);
                }
                if (nameData == "UserInsights")
                {
                    var js = JsonConvert.DeserializeObject<InfluencersMetricsService.Model.UserInsights>(json);
                    await repMongo.GravarOne<InfluencersMetricsService.Model.UserInsights>(js);
                }
                if (nameData == "InsightsCity")
                {
                    var js = JsonConvert.DeserializeObject<Models.Graph.InsightsGenderAge>(json);
                    await repMongo.GravarOne<Models.Graph.InsightsGenderAge>(js);
                }
                if (nameData == "InsightsGenderAge")
                {
                    var js = JsonConvert.DeserializeObject<Models.DTO.Insigth>(json);
                    var lstIns = new List<Models.DTO.InsigthDTO>();
                    js.data.ForEach(f =>
                    {
                        lstIns.Add(new Models.DTO.InsigthDTO()
                        {
                            data = new List<Models.DTO.DatumDTO>()
                                    {
                                    new Models.DTO.DatumDTO()
                                    {
                                        description = f.description,
                                        id = f.id,
                                        name = f.name,
                                        period = f.period,
                                        title = f.title,
                                        values = f.values.Select(s=>new Models.DTO.ValueDTO()
                                        {
                                            end_time = s.end_time,
                                            value = s.value.Select(sv=> new Models.DTO.ValueName()
                                            {
                                                name = sv.Key,
                                                valor = sv.Value.ToString()
                                            }).ToList()
                                        }).ToList()
                                    }
                                    }
                        });
                    });
                    await repMongo.GravarOne<Models.DTO.InsigthDTO>(lstIns.FirstOrDefault());
                }
                return "";
            }
            catch (Exception)
            {
                return "Error#" + nameData;
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> setDataStories([FromBody]
            InfluencersMetricsService.Model.Request<InfluencersMetricsService.Model.Stories> request)
        {
            var json = request.Obj;
            var key = request.userId;
            if (json == null)
            {
                return "Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(key))
            {
                return "chave não encontrados.";
            }

            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, key);
                await repMongo.GravarOne<InfluencersMetricsService.Model.Stories>(json);
                return "";
            }
            catch (Exception)
            {
                return "Error#setDataStories";
            }
        }

        [HttpGet]
        public async Task<ActionResult<string>> setCorrecoes(DateTime dt)
        {
            Repository.MongoRep repMongoList = new Repository.MongoRep("MetricasInsights", _appSettings, "");
            //listar e processar o maximo possivel
            var lst = await repMongoList.ListarTotalListObjects<InfluencersMetricsService.Model.StoryInsights>(dt);

            //chamar setDataStoriesInsights
            foreach (var l in lst)
            //Parallel.ForEach(lst, l =>
            {
                try
                {
                    var json = l.Obj;
                    var key = l.UsuarioId;
                    Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, key);

                    //Gravar Insights de Stories no Max
                    Parallel.ForEach(json.data, f =>
                    {
                        try
                        {
                            var local = dt;
                            var result = repMongo.SetStoryInsightResume(new ResumeStoryMidia()
                            {
                                id = (f.id.Split('/')[0]).ToString().ToUpper(),
                                description = f.description,
                                name = f.name,
                                period = f.period,
                                title = f.title,
                                values = f.values
                            }).Result;
                        }
                        catch (Exception)
                        {

                        }
                    });
                }
                catch (Exception)
                {

                }
            };//);

            return null;
        }

        [HttpPost]
        public async Task<ActionResult<string>>
            setDataStoriesInsights([FromBody] InfluencersMetricsService.Model.Request<InfluencersMetricsService.Model.StoryInsights> request)
        {
            var json = request.Obj;
            var key = request.userId;
            if (json == null)
            {
                return "Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(key))
            {
                return "usuario e chave não encontrados.";
            }

            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, key);
                await repMongo.GravarOne<InfluencersMetricsService.Model.StoryInsights>(json);

                //Gravar Insights de Stories no Max
                foreach (var f in json.data)
                //Parallel.ForEach(json.data, f =>
                {
                    var result = repMongo.SetStoryInsightResume(new ResumeStoryMidia()
                    {
                        id = (f.id.Split('/')[0]).ToString().ToUpper(),
                        description = f.description,
                        name = f.name,
                        period = f.period,
                        title = f.title,
                        values = f.values
                    }).Result;
                };//);

                return "";
            }
            catch (Exception)
            {
                return "Error#setDataStoriesInsights";
            }
        }


        [HttpPost]
        public async Task<ActionResult<string>> setUserBloqueio([FromBody] InfluencersMetricsService.Model.Request<UserBloqueios> request)
        {
            var key = request.userId;
            var json = request.Obj;

            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, key);
                await repMongo.GravarOne<UserBloqueios>(json);
                return "";
            }
            catch (Exception)
            {
                return "Error#setUserBloqueio";
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> setDataUsersInsights([FromBody]
            InfluencersMetricsService.Model.Request<InfluencersMetricsService.Model.UserInsights> request)
        {
            var json = request.Obj;
            var key = request.userId;
            var obj = request.Obj.ToString();
            if (json == null)
            {
                return "Não recebemos o valor dos insigths.";
            }
            if (string.IsNullOrEmpty(key))
            {
                return "usuario e chave não encontrados.";
            }

            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, key);
                await repMongo.GravarOne<InfluencersMetricsService.Model.UserInsights>(json);
                return "";
            }
            catch (Exception)
            {
                return "Error#setDataUsersInsights";
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> loadData(string json, string usuario, string key)
        {
            if (string.IsNullOrEmpty(json))
            {
                return "Não recebemos o valor dos insigths.";
            }

            Repository.MongoRep repMongo = new Repository.MongoRep(usuario, _appSettings);
            try
            {
                var lstMetricas = await repMongo.GetMetrica(usuario, key);
                if (lstMetricas == null || (lstMetricas.Count == 0))
                {
                    return "Autorização de métrica não encontrada.";
                }
                else
                {
                    var autorizacao = lstMetricas.FirstOrDefault();
                    if (autorizacao.Obj.Status == null || (autorizacao.Obj.Status != EnumStatus.PROCESSADO))
                    {
                        usuario = autorizacao.UsuarioInstagram;
                        var js = JsonConvert.DeserializeObject<Models.DTO.Insigth>(json);
                        var lstIns = new List<Models.DTO.InsigthDTO>();
                        js.data.ForEach(f =>
                        {
                            lstIns.Add(new Models.DTO.InsigthDTO()
                            {
                                data = new List<Models.DTO.DatumDTO>()
                                {
                                    new Models.DTO.DatumDTO()
                                    {
                                        description = f.description,
                                        id = f.id,
                                        name = f.name,
                                        period = f.period,
                                        title = f.title,
                                        values = f.values.Select(s=>new Models.DTO.ValueDTO()
                                        {
                                            end_time = s.end_time,
                                            value = s.value.Select(sv=> new Models.DTO.ValueName()
                                            {
                                                name = sv.Key,
                                                valor = sv.Value.ToString()
                                            }).ToList()
                                        }).ToList()
                                    }
                                }
                            });
                        });
                        repMongo = new Repository.MongoRep(usuario, _appSettings);
                        await repMongo.GravarOne<Models.DTO.InsigthDTO>(lstIns.FirstOrDefault());
                        autorizacao.Obj.Status = "Processando";

                        await repMongo.AlterarMetrica(autorizacao);
                        usuario = autorizacao.UsuarioInstagram;
                    }
                    else
                    {
                        return "Autorização já utilizada.";
                    }
                }
            }
            catch (Exception)
            {
                return "Erro ao processar insigths.";
            }

            return "usuario:'" + usuario + "'";
        }

        [HttpPost]
        public async Task<string> Processado(string usuario, string key)
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep(usuario, _appSettings);
                var autorizacao = new Models.ContractClass<Models.AutorizacaoMetrica>
                {
                    Obj = new Models.AutorizacaoMetrica
                    {
                        Status = EnumStatus.PROCESSADO,
                        Key = key
                    }
                };

                await repMongo.AlterarMetrica(autorizacao);

                return "";
            }
            catch (Exception)
            {
                return "Erro ao processar autorização.";
            }
        }

        public ActionResult Testes()
        {
            return View();
        }

        private async Task<ActionResult> ConsultarInstagram(Models.Usuario usuario)
        {
            //var user = await insta.GetUserAsync(usuario.UsuarioInstagram.ToString().ToLower());
            //var lstUserMedia = await insta.GetUserMediaAsync(usuario.UsuarioInstagram.ToString().ToLower(),                            PaginationParameters.MaxPagesToLoad(2));
            //var lstMentionsTags = await insta.GetUserTagsAsync(usuario.UsuarioInstagram.ToString().ToLower(),                                PaginationParameters.MaxPagesToLoad(2));
            //var lstFollowing = await insta.GetUserFollowingAsync(usuario.UsuarioInstagram.ToString().ToLower(),                                PaginationParameters.MaxPagesToLoad(1));
            //var lstMediasUser = await insta.GetUserMediaAsync(itUser, PaginationParameters.MaxPagesToLoad(1));
            return null;
        }

        public async Task<ActionResult> Passos()
        {
            return View();
        }

        public ActionResult Cities()
        {
            return View();
        }

        public async Task<Models.FaceDetection> getAnnotation(string _urlImagem)
        {
            string value = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            //var repMongo = new Repository.MongoRep("", _appSettings, userId);
            var _mediaId = Guid.NewGuid().ToString();
            try
            {
                var desc = string.Empty;
                WebClient webClient = new WebClient();
                var remoteFileUrl = _urlImagem;
                var _urlLocal = @"c:\output\" + _mediaId + "_imgEmotional.jpg";
                webClient.DownloadFile(remoteFileUrl, _urlLocal);

                int count = 1;
                int joy = 0;
                int anger = 0;
                int sorrow = 0;
                int surprise = 0;
                var response = GetAnnotationDetectionFace(_urlLocal);
                if (response != null && (response.responses != null) && (response.responses.Count > 0))
                {
                    if (response.responses.Where(w => w.faceAnnotations != null).Count() > 0)
                    {
                        foreach (var faceAnnotation in response.responses.FirstOrDefault().faceAnnotations)
                        {
                            if (faceAnnotation != null)
                            {
                                Console.WriteLine("Face {0}:", count++);
                                joy += LikedFace.ConvertFace(faceAnnotation.joyLikelihood);
                                anger += LikedFace.ConvertFace(faceAnnotation.angerLikelihood);
                                sorrow += LikedFace.ConvertFace(faceAnnotation.sorrowLikelihood);
                                surprise += LikedFace.ConvertFace(faceAnnotation.surpriseLikelihood);
                            }
                        }
                    }
                }

                var it = new Models.FaceDetection()
                {
                    Joy = joy == 0 ? 0 : 1,
                    Anger = anger == 0 ? 0 : 1,
                    Sorrow = sorrow == 0 ? 0 : 1,
                    Surprise = surprise == 0 ? 0 : 1,
                    DtAvaliacao = DateTime.Now,
                    UrlImagem = _urlImagem,
                    MediaId = _mediaId
                };
                return it;
            }
            catch (Exception ex)
            {
                //await repMongo.GravarOne<string>("Exception em getAnnotation:: " + ex.Message.ToString() + ":: stacktrace::" + ex.StackTrace.ToString());
                if (ex.InnerException != null)
                {
                    //  await repMongo.GravarOne<string>(ex.InnerException.Message.ToString());
                }

                var it = new Models.FaceDetection()
                {
                    Joy = 0,
                    Anger = 0,
                    Sorrow = 0,
                    Surprise = 0,
                    DtAvaliacao = DateTime.Now,
                    UrlImagem = _urlImagem,
                    MediaId = _mediaId
                };
                return it;
            }
        }

        public Models.AnnotationFaceDetection GetAnnotationDetectionFace(string _urlLocal)
        {
            var key = "AIzaSyD5ubW6Dwv0NgRYbWzeh2dCMhZ9dBmcGEc";// "AIzaSyBwInxtzdKkaXWT8wysGFVrKHBGYM4wdtU";
            var req = new Models.FaceDetection();
            var base64String = "";
            byte[] imageBytes;

            try
            {
                using (Image image = Image.FromFile(_urlLocal))
                {
                    using (var m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        base64String = Convert.ToBase64String(imageBytes);
                    }
                }

                var imageParts = base64String.Split(',').ToList<string>();
                using (var client = new WebClient())
                {
                    var Mainrequests = new Models.OptionRequestFaceDetection()
                    {
                        requests = new List<Models.requests>()
                        {
                             new Models.requests()
                            {
                                 image = new Models.image()
                                 {
                                    content = imageParts[0]
                                 },

                                 features = new List<Models.features>()
                                 {
                                     new Models.features()
                                     {
                                         type = "FACE_DETECTION"
                                     }
                                 }
                            }
                        }
                    };
                    var uri = "https://vision.googleapis.com/v1/images:annotate?key=" + key;
                    client.Headers.Add("Content-Type:application/json");
                    client.Headers.Add("Accept:application/json");
                    var response = client.UploadString(uri, JsonConvert.SerializeObject(Mainrequests));
                    var ret = Json(data: response);

                    var retClass = JsonConvert.DeserializeObject<Models.AnnotationFaceDetection>(response.ToString());
                    return retClass;
                }
            }
            catch (Exception ex)
            {
                var repMongo = new Repository.MongoRep("", _appSettings);
                repMongo.GravarOne<string>(ex.Message.ToString() + "::" + ex.StackTrace.ToString());

                return null;
            }
        }

        public async Task<string> GetDiscovery(string accesstoken, string usuarios)
        {
            var item = await GetDataGraphAsync<StoryInsights>(accesstoken,
                "?fields=business_discovery.username(" + usuarios + "){followers_count,media_count,media{comments_count,like_count,caption,media_url,timestamp}}");
            return "";
        }

        [HttpPost]
        public async Task SetStoryInsight(string idUsuarioInstagram, string _userId, string accesstoken = "")
        {
            var _access_token = "";
            Repository.MongoRep repMongo = new Repository.MongoRep("MetricasInsights", _appSettings, _userId);

            if (accesstoken == "")
            {
                _access_token = (await repMongo.Listar<Models.Usuario>(idUsuarioInstagram)).Where(w => w._id.ToString() == _userId).FirstOrDefault().Obj.access_token_page;
            }
            else
            {
                _access_token = accesstoken;
            }

            //Lendo todos os Stories
            var result = GetDataGraphAsync<Stories>(_access_token, idUsuarioInstagram + "/stories?fields=media_url,id,permalink,media_type,caption,username,timestamp").Result;

            //Detalhe de cada Stories
            if (result != null && (result.data.Count > 0))
            {
                WebClient webClient = new WebClient();
                await repMongo.GravarOne<InfluencersMetricsService.Model.Stories>(result);

                foreach (var it in result.data)
                {
                    try
                    {
                        if (it.media_type == "IMAGE")
                        {
                            var remoteFileUrl = it.media_url;
                            var _urlLocal = @"c:\output\" + it.id + ".jpg";
                            if (!System.IO.File.Exists(_urlLocal))
                            {
                                webClient.DownloadFile(remoteFileUrl, _urlLocal);
                                var resultName = UploadFile.UploadFileLoad(_urlLocal, it.id + ".jpg");
                            }
                        }

                        if (it.media_type == "VIDEO")
                        {
                            var remoteFileUrl = it.media_url;
                            var _urlLocal = @"c:\output\" + it.id + ".MP4";
                            if (!System.IO.File.Exists(_urlLocal))
                            {
                                webClient.DownloadFile(remoteFileUrl, _urlLocal);
                                var resultName = UploadFile.UploadFileLoad(_urlLocal, it.id + ".MP4");
                                if (!string.IsNullOrEmpty(resultName))
                                {
                                    System.IO.File.Delete(_urlLocal);
                                }
                            }
                        }

                        var item = GetDataGraphAsync<StoryInsights>(_access_token, it.id + "/insights?metric=exits,impressions,reach,replies,taps_forward,taps_back").Result;
                        await repMongo.GravarOne<StoryInsights>(item);
                    }
                    catch
                    {

                    }
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> GravarNovoUsuario([FromBody] string chave)
        {
            return Ok(true);
        }

        public async Task<ActionResult> ConsultaBasica(string influencer)
        {
            var insta = await IniciarInstaSharper("danieljromualdo", "28D@niel2020");
           
            if (insta == null)
            {
                ViewBag.jsExecutar = "alert('Sua senha não confere.<br />Por favor digite sua senha corretamente');";
                return RedirectToAction("AutorizarMetricaError", "relatorios", new Models.DTO.Erro() { Message = "Sua senha não confere.<br />Por favor digite sua senha corretamente" });
            }

            var lstUserMedia = await insta.GetUserMediaAsync(influencer.ToLower(),
                PaginationParameters.MaxPagesToLoad(2));
            if (lstUserMedia.Succeeded == true)
            {
                var user = await insta.GetUserAsync(influencer.ToLower());

                var lstMentionsTags = await insta.GetUserTagsAsync(influencer.ToLower(), PaginationParameters.MaxPagesToLoad(2));

                var lstFollowing = await insta.GetUserFollowingAsync(influencer.ToLower(), PaginationParameters.MaxPagesToLoad(1));
            }

            return View();
        }

        private async static Task<IInstaApi> IniciarInstaSharper(string usuario, string senha)
        {
            try
            {
                // create user session data and provide login details
                var userSession = new UserSessionData
                {
                    UserName = usuario,
                    Password = senha
                };

                var _instaApi = InstaApiBuilder.CreateBuilder()
                        .SetUser(userSession)

                        .UseLogger(new DebugLogger(LogLevel.All)) // use logger for requests and debug messages
                        .SetRequestDelay(TimeSpan.FromSeconds(2))
                        .Build();

                var logInResult = await _instaApi.LoginAsync();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    throw new Exception("Informações do usuário estão inválidas <br /> Vá em seu perfil e complete suas informações de cadastro");
                }

                return _instaApi;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class _request
    {
        public string userId { get; set; }
        public string mediaId { get; set; }
        public int impressions { get; set; }
        public int reach { get; set; }
        public int saved { get; set; }
        public int engagement { get; set; }
        public string mediaGraphId { get; set; }
    };
    public class _reqJsonData
    {
        public string json { get; set; }
        public string namePage { get; set; }
        public string key { get; set; }
        public string nameData { get; set; }
    }
}