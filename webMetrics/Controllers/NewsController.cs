using InstaSharper.Classes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using webMetrics.Models;
using webMetrics.Models.DTO;

namespace webMetrics.Controllers
{
    public class NewsController : Controller
    {
        private readonly double valor1 = 5.9;
        private readonly double valor0 = 2.1;
        private readonly IOptions<Models.AppSettings> _settings;

        public NewsController(IOptions<Models.AppSettings> appSettings)
        {
            _settings = appSettings;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AutorizarMetrica()
        {
            return View();
        }
        public IActionResult DisasterCheck()
        {
            return View();
        }
        public IActionResult DisasterCheckResult()
        {
            return View();
        }
        public IActionResult EuQuero()
        {
            return View();
        }

        public async Task<ActionResult> MidiaKit(string key)
        {
            try
            {
                var _id = new ObjectId(key);
                var UserId = HttpContext.Session.GetString("UserId");
                Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);

                var lstMongoUser = await repMongo.ListarById<Models.Graph.Usuario>(_id);
                var mongoUser = lstMongoUser.ToList();
                var userId = lstMongoUser.FirstOrDefault().UsuarioId;
                repMongo = new Repository.MongoRep(UserId, _settings, userId);
                ViewBag.IdPage = _id;

                return await LoadMetricas(mongoUser, repMongo, userId, "Metricas");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }

        }

        private async Task<ActionResult> LoadMetricas(List<ContractClass<Models.Graph.Usuario>> mongoUser,
            Repository.MongoRep repMongo, string UserId, string _nameView)
        {
            var linhaerro = "";
            var inf = new Models.DTO.InfluencersResumoFree();
            var metricas = new Models.Metricas();
            try
            {
                #region Repositorios
                linhaerro = "Repositorios";
                var objUser = mongoUser.FirstOrDefault().Obj;
                var dtCriacao = mongoUser.FirstOrDefault().DateCreation;

                var lstMongoMedias = await repMongo.Listar<Models.Graph.Media>(UserId);
                List<Models.Graph.Datum> mongoMediasUnico = new List<Models.Graph.Datum>();
                var mongoMedias = lstMongoMedias.Where(w => w.DateCreation == dtCriacao && w.Obj != null && (w.Obj.data != null))
                    .Select(s => s.Obj.data).ToList();
                if (mongoMedias.Count() > 0)
                {
                    mongoMediasUnico = mongoMedias.FirstOrDefault()
                        .Where(w => w.timestamp >= dtCriacao.AddDays(-8)).ToList();
                }

                var lstMongoMentions = new List<webMetrics.Models.Graph.Datum>();
                foreach (var itMentions in mongoMediasUnico)
                {
                    if (itMentions.caption != null)
                    {
                        if (itMentions.caption.IndexOf(@"@") > -1)
                        {
                            lstMongoMentions.Add(itMentions);
                        }
                    }
                }

                var lstMongoTags = await repMongo.Listar<Models.Graph.Tags>(UserId);
                List<Models.Graph.Datum> mongoTagsUnico = new List<Models.Graph.Datum>();
                var mongoTags = lstMongoTags.Where(w => w.DateCreation == dtCriacao && w.Obj != null && (w.Obj.data != null))
                    .Select(s => s.Obj.data).ToList();
                if (mongoTags.Count() > 0)
                {
                    mongoTagsUnico = mongoTags.FirstOrDefault()
                        .Where(w => w.timestamp >= dtCriacao.AddDays(-8)).ToList();
                }

                var lstCities = await repMongo.Listar<Models.Graph.InsightsGenderAge>(UserId);
                var mongoCities = lstCities.Where(w => w.DateCreation == dtCriacao).ToList();
                var lstObjCities = mongoCities.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault().values[0].value,
                    timeSpan = s.timeSpan
                }
                ).ToList();

                var insigths = await repMongo.Listar<Models.DTO.InsigthDTO>(UserId);
                var lstInsigthsAge = insigths.ToList();
                var lstObjAges = lstInsigthsAge.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault(),
                    timeSpan = s.timeSpan
                }
                ).ToList();
                var lstAgesFull = lstObjAges.Where(w => w.data.title.Contains("Gender") || w.data.title.Contains("Gênero"));
                var lstAge = lstAgesFull.Where(w => w.timeSpan == lstAgesFull.Max(m => m.timeSpan)).FirstOrDefault();

                var objMinhasMidias = mongoMediasUnico;//.FirstOrDefault();
                var objMinhasMidiasSemanal = objMinhasMidias.Where(x => x.timestamp >= dtCriacao.AddDays(-7)).ToList();
                var objEngaj = objMinhasMidias.OrderByDescending(x => x.timestamp).Take(5).ToList();

                var lstStoryGraphs = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.StoryInsights>(UserId, dtCriacao);
                var lstStoryIdsTmp = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.Stories>(UserId, dtCriacao);
                lstStoryIdsTmp = lstStoryIdsTmp.Where(w => w.DateCreation >= dtCriacao && w.DateCreation <= dtCriacao.AddDays(7)).ToList();
                var _lstStoryIds = new List<string>();
                lstStoryIdsTmp.ForEach(f =>
                {
                    f.Obj.data.ForEach(fi =>
                    {
                        _lstStoryIds.Add(fi.id + "|" + fi.media_type);
                    });
                });
                var lstStoryIdTypes = _lstStoryIds.DistinctBy(d => d);
                _lstStoryIds = _lstStoryIds.DistinctBy(d => d).ToList();
                var objUserInsigths = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.UserInsights>(UserId);

                var objBestHourInsights = await repMongo.ListarGraphUserId<List<InfluencersMetricsService.Model.StoryUserBestHour>>(UserId);

                var faceDetection = await repMongo.ListarGraphUserId<List<Models.FaceDetection>>(UserId);
                var lstFaceDetection = faceDetection.Where(w => w.timeSpan == faceDetection.Max(x => x.timeSpan)).ToList();

                var newUserInsightWeekend = new Models.UserInsightWeekend();
                try
                {
                    //Verificar se já existe
                    var exist = (await repMongo.Listar<Models.UserInsightWeekend>(UserId));
                    if (exist == null || (exist.FirstOrDefault() == null || (exist.FirstOrDefault().Obj == null)))
                    {
                        var seguidores = Convert.ToInt32(objUser.followers_count);
                        var usuario = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
                        var access_token = usuario.FirstOrDefault().Obj.access_token_page;
                        var _instagram_business_account = usuario.FirstOrDefault().Obj.UsuarioInstagram;

                        var dtInicial = Helper.ConvertToTimestamp(dtCriacao.AddDays(-14));
                        var dtFinal = Helper.ConvertToTimestamp(dtCriacao.AddDays(-8));
                        var resultGrowth = Helper.GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>
                            (access_token, _instagram_business_account + "/insights?until=" + dtFinal.ToString() +
                            "&since=" + dtInicial.ToString() + "&period=day&metric=follower_count").Result;
                        var initial = 0;
                        resultGrowth.data[0].values.ForEach(f =>
                        {
                            initial += f.value;
                        });

                        dtInicial = Helper.ConvertToTimestamp(dtCriacao.AddDays(-7));
                        dtFinal = Helper.ConvertToTimestamp(dtCriacao);
                        resultGrowth = Helper.GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>
                            (access_token, _instagram_business_account + "/insights?until=" + dtFinal.ToString() +
                            "&since=" + dtInicial.ToString() + "&period=day&metric=follower_count").Result;
                        var final = 0;
                        resultGrowth.data[0].values.ForEach(f =>
                        {
                            final += f.value;
                        });

                        var newInsight = new Models.UserInsightWeekend()
                        {
                            Initial = initial,
                            Final = final,
                            PercentFinal = Math.Round((Convert.ToDecimal(final) / Convert.ToDecimal(seguidores) * 100), 2),
                            PercentInitial = Math.Round((Convert.ToDecimal(initial) / Convert.ToDecimal(seguidores) * 100), 2)
                        };
                        await repMongo.GravarOne<Models.UserInsightWeekend>(newInsight);
                        newUserInsightWeekend = newInsight;
                    }
                    else
                    {
                        newUserInsightWeekend = exist.FirstOrDefault().Obj;
                    }
                    ViewBag.GrowthWeekPeriod =
                    dtCriacao.AddDays(-14).ToString("dd/MM") + "-" + dtCriacao.AddDays(-8).ToString("dd/MM") + " | " +
                    dtCriacao.AddDays(-7).ToString("dd/MM") + "-" + dtCriacao.ToString("dd/MM");
                    ViewBag.GrowthWeek = newUserInsightWeekend.Initial.ToString() + " - " + newUserInsightWeekend.Final.ToString();
                    ViewBag.GrowthWeekPercent = newUserInsightWeekend.PercentInitial.ToString() + "% - " + newUserInsightWeekend.PercentFinal.ToString() + "%";
                }
                catch (Exception ex)
                {
                    newUserInsightWeekend = new Models.UserInsightWeekend();
                }

                #endregion

                #region Sumario
                linhaerro = "Novo Sumario";
                var mediaStoryVideo = 0;
                var mediaStoryImage = 0;
                var percentmediaStoryVideo = 0d;
                var percentmediaStoryImage = 0d;

                var infRepliesTotal = 0;
                var infComentariosTotal = 0;
                var comentarios = 0;
                objMinhasMidias.ForEach(x =>
                {
                    comentarios += (Convert.ToInt32(x.comments_count));
                });
                inf.Comentarios = comentarios;

                metricas.Cabecalho = new Cabecalho
                {
                    Audienciacrescimento = 0,
                    Mediaalcance = 0,
                    Mediaengajamento = 0,
                    Mediaimpressoes = 0,
                    Reach = Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / objMinhasMidias.Count),
                    Seguidores = Convert.ToInt32(objUser.followers_count),
                    Seguindo = Convert.ToInt32(objUser.follows_count),
                    Viewperfil = 0
                };

                metricas.Sumario = new Sumario
                {
                    Avgpostsdiario = 0,
                    Avgpostsmensal = 0,
                    Avgpostssemanal = 0,
                    Comentariosseguidores = 0,
                    Fotospostfeed = 0,
                    Fotosstories = 0,
                    Mediacomentarios = 0,
                    Medialikes = 0,
                    Mediastories = 0,
                    Posts = objMinhasMidias.Count(),
                    Seguindoseguidores = metricas.Cabecalho.Seguindo / metricas.Cabecalho.Seguidores * 100,
                    Totalcomentarios = comentarios,
                    Totallikes = objMinhasMidias.Sum(x => x.like_count),
                    Videospostfeed = 0,
                    Videosstories = 0
                };

                linhaerro = "Sumario";
                inf.Seguidores = Convert.ToInt32(objUser.followers_count);
                inf.Seguindo = Convert.ToInt32(objUser.follows_count);
                inf.SeguindoSeguidores = inf.Seguidores > 0 ? inf.Seguindo / (decimal)inf.Seguidores * 100 : 0;
                inf.Posts = objMinhasMidias.Count();
                inf.Curtidas = objMinhasMidias.Sum(x => x.like_count);
                inf.ProfilePicture = objUser.profile_picture_url;
                inf.NomeCompleto = objUser.name;
                inf.UserName = objUser.username;
                inf.SocialContext = objUser.biography;


                inf.avgPostReach = objMinhasMidias.Count == 0 ? 0 : Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / objMinhasMidias.Count);

                inf.MediaCurtidas = inf.Posts == 0 ? 0 : inf.Curtidas / inf.Posts;
                inf.MediaComentarios = inf.Posts == 0 ? 0 : inf.Comentarios / (decimal)inf.Posts;
                inf.ComentariosSeguidores = inf.Seguidores > 0 ? (inf.MediaComentarios / inf.Seguidores) * 100 : 0;
                inf.Engajamento = inf.Posts == 0 ? 0 : (
                    (inf.Curtidas + (decimal)inf.Comentarios) / inf.Posts) / inf.Seguidores * 100;
                inf.Alcance = inf.Curtidas + inf.Comentarios;
                inf.MediaAlcancePost = inf.Posts == 0 ? 0 : inf.Posts / (decimal)inf.Alcance * 100;
                inf.Aprovado = 2;

                if (objUserInsigths.Count > 0)
                {
                    objUserInsigths = objUserInsigths.Where(w => w.Obj != null).ToList();
                    var dataUserInsigths = objUserInsigths.Select(s => new
                    {
                        Data = s.Obj.data,
                        Tipo = s.Obj.data.Exists(e => e.name == "reach") ? "O" : (s.Obj.data.Exists(e => e.name != "follower_count") ? "P" : "F"),
                        DateCreation = s.DateCreation
                    }).ToList();

                    var _impressions = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "impressions").FirstOrDefault();
                    var _reach = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "reach").FirstOrDefault();
                    inf.Impressions = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "impressions").FirstOrDefault().values.FirstOrDefault().value;
                    inf.Reach = _reach.values.FirstOrDefault().value;

                    inf.PeriodImpressions = (_impressions.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    inf.PeriodReach = (_reach.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");

                    ViewBag.GrowthData = "";
                    if (dataUserInsigths.Where(o => o.Tipo == "P").Count() > 0)
                    {
                        var _profileviews = dataUserInsigths.Where(o => o.Tipo == "P").FirstOrDefault().Data.Where(w => w.name == "profile_views").FirstOrDefault();
                        inf.ProfileViews = _profileviews.values.FirstOrDefault().value;
                        inf.PeriodProfileViews = (_profileviews.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        var _profileviews = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "profile_views").FirstOrDefault();
                        inf.ProfileViews = _profileviews.values.FirstOrDefault().value;
                        inf.PeriodProfileViews = (_profileviews.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    }

                    ViewBag.GrowthData = "[]";
                    ViewBag.GrowthCategoria = "[]";

                    var TYPEFILTRO = "F";
                    if (dataUserInsigths.Where(o => o.Tipo == TYPEFILTRO).Count() > 0)
                    {
                        var lstGrowthCategorias = "";
                        var lstGrowthValores = "";
                        var contador = 0;
                        var lstMeses = new List<PeriodoValor>();
                        foreach (var it in dataUserInsigths.Where(o => o.Tipo == TYPEFILTRO && o.DateCreation >= dtCriacao)
                            .OrderByDescending(d => d.DateCreation).ToList())
                        {
                            foreach (var dat in it.Data)
                            {
                                foreach (var _values in dat.values)
                                {
                                    lstGrowthCategorias += "'" + it.DateCreation.AddDays(-(contador++)).ToString("dd/MM/yy") + "',";
                                    lstGrowthValores += _values.value + ",";
                                    lstMeses.Add(new PeriodoValor()
                                    {
                                        Mes = it.DateCreation.AddDays(-(contador++)).ToString("MM/yyyy"),
                                        Valor = _values.value
                                    });
                                }
                                break;
                            }
                            break;
                        }
                        var somaValores = lstMeses.Sum(s => s.Valor);
                        var resultValores = lstMeses
                            .GroupBy(x => x.Mes)
                            .Select(g => new PeriodoValor
                            {
                                Valor = g.Sum(x => x.Valor > 0 ? x.Valor : 0),
                                Mes = g.First().Mes,
                                Perc = Convert.ToInt32(
                                            Convert.ToDouble(
                                                Convert.ToDouble(g.Sum(x => x.Valor > 0 ? x.Valor : 0))
                                                /
                                                Convert.ToDouble(somaValores)
                                            )
                                        *
                                        100)
                            }).OrderBy(o => o.Mes).ToList();
                        inf.LstValoresMeses = resultValores;

                        ViewBag.GrowthData = lstGrowthValores;
                        ViewBag.GrowthCategoria = lstGrowthCategorias + "";
                    }

                    TYPEFILTRO = "O";
                    if (dataUserInsigths.Where(o => o.Tipo == TYPEFILTRO).Count() > 0)
                    {
                        var contador = 0;
                        var lstMeses = new List<PeriodoValor>();
                        foreach (var it in dataUserInsigths.Where(o => o.Tipo == TYPEFILTRO && o.DateCreation >= dtCriacao)
                            .OrderByDescending(d => d.DateCreation).ToList())
                        {
                            foreach (var dat in it.Data)
                            {
                                foreach (var _values in dat.values)
                                {
                                    lstMeses.Add(new PeriodoValor()
                                    {
                                        Mes = it.DateCreation.AddDays(-(contador++)).ToString("MM/yyyy"),
                                        Valor = _values.value
                                    });
                                }
                                break;
                            }
                            break;
                        }
                        var somaValores = lstMeses.Sum(s => s.Valor);
                        var resultValores = lstMeses
                            .GroupBy(x => x.Mes)
                            .Select(g => new PeriodoValor
                            {
                                Valor = g.Sum(x => x.Valor > 0 ? x.Valor : 0),
                                Mes = g.First().Mes,
                                Perc = Convert.ToInt32(
                                            Convert.ToDouble(
                                                Convert.ToDouble(g.Sum(x => x.Valor > 0 ? x.Valor : 0))
                                                /
                                                Convert.ToDouble(somaValores)
                                            )
                                        *
                                        100)
                            }).OrderBy(o => o.Mes).ToList();
                        inf.LstValoresMesesO = resultValores;
                    }

                    TYPEFILTRO = "P";
                    if (dataUserInsigths.Where(o => o.Tipo == TYPEFILTRO).Count() > 0)
                    {
                        var contador = 0;
                        var lstMeses = new List<PeriodoValor>();
                        foreach (var it in dataUserInsigths.Where(o => o.Tipo == TYPEFILTRO && o.DateCreation >= dtCriacao)
                            .OrderByDescending(d => d.DateCreation).ToList())
                        {
                            foreach (var dat in it.Data)
                            {
                                foreach (var _values in dat.values)
                                {
                                    lstMeses.Add(new PeriodoValor()
                                    {
                                        Mes = it.DateCreation.AddDays(-(contador++)).ToString("MM/yyyy"),
                                        Valor = _values.value
                                    });
                                }
                                break;
                            }
                            break;
                        }
                        var somaValores = lstMeses.Sum(s => s.Valor);
                        var resultValores = lstMeses
                            .GroupBy(x => x.Mes)
                            .Select(g => new PeriodoValor
                            {
                                Valor = g.Sum(x => x.Valor > 0 ? x.Valor : 0),
                                Mes = g.First().Mes,
                                Perc = Convert.ToInt32(
                                            Convert.ToDouble(
                                                Convert.ToDouble(g.Sum(x => x.Valor > 0 ? x.Valor : 0))
                                                /
                                                Convert.ToDouble(somaValores)
                                            )
                                        *
                                        100)
                            }).OrderBy(o => o.Mes).ToList();
                        inf.LstValoresMesesP = resultValores;
                    }
                    inf.mediaValoresMesesP = inf.LstValoresMesesP==null? 0: inf.LstValoresMesesP.Average(a => a.Perc) - 100;
                    inf.mediaValoresMesesO = inf.LstValoresMesesO == null ? 0 :  inf.LstValoresMesesO.Average(a => a.Perc) - 100;
                    inf.mediaValoresMeses = inf.LstValoresMeses == null ? 0 :  inf.LstValoresMeses.Average(a => a.Perc) - 100;

                }
                #endregion

                #region BestHour
                ViewBag.BestHourCoordenates = "[]";
                if (objBestHourInsights.Count > 0)
                {
                    var lstBestHourChart = new List<InfluencersMetricsService.Model.StoryUserBestHour>();

                    var lstBestHour = objBestHourInsights.FirstOrDefault().Obj;
                    foreach (var dia in lstBestHour.Select(s => s.DiaDaSemana).Distinct())
                    {
                        foreach (var hour in lstBestHour.Where(w => w.DiaDaSemana == dia).Select(s => s.Hour).Distinct().OrderBy(x => x))
                        {
                            var valorReach = 0;
                            var qtd = 0;
                            lstBestHour.Where(w => w.DiaDaSemana == dia && w.Hour == hour).ForEach(f =>
                            {
                                qtd++;
                                valorReach += f.ValorReach;
                            });

                            lstBestHourChart.Add(new InfluencersMetricsService.Model.StoryUserBestHour()
                            {
                                Hour = hour,
                                DiaDaSemana = dia,
                                ValorReach = valorReach / qtd
                            });
                        }
                    }

                    var strBestHour = "[";
                    lstBestHourChart.ForEach(f =>
                    {
                        strBestHour += "[" + f.Hour.ToString() + "," + ((int)f.DiaDaSemana).ToString() + "," + f.ValorReach.ToString() + "],";
                    });
                    strBestHour += "]";
                    ViewBag.BestHourCoordenates = strBestHour;

                    string MapTo(int maxValue, int maxResult, int value)
                    {
                        var onePercent1 = 100m / maxValue;
                        var percent1 = value * onePercent1;
                        var onePercent2 = maxResult / 100m;
                        var result = percent1 * onePercent2;
                        return result.ToString("N0");
                    }

                    var maxReach = lstBestHourChart.Max(a => a.ValorReach);
                    var bestHoursChartData = lstBestHourChart.Select(a =>
                    {
                        return new string[]
                        {
                            (1 + (int)a.DiaDaSemana).ToString(),
                            a.Hour.ToString(),
                            MapTo(maxReach, 125, a.ValorReach)
                        };
                    }).ToList();
                    ViewBag.BestHourChartData = bestHoursChartData;

                    var best = lstBestHourChart.Where(w => w.ValorReach == lstBestHourChart.Max(m => m.ValorReach)).FirstOrDefault();
                    @ViewBag.BestDay = best.DiaDaSemana.ToString().ToUpper();
                    @ViewBag.BestTime = best.Hour;
                }
                #endregion

                #region Calculo de Engajamento 
                linhaerro = "Engajamento";
                var engComentarios = 0;
                engComentarios = Convert.ToInt32(inf.Comentarios);
                var engCurtidas = inf.Curtidas;

                var mediaEngaj = Convert.ToDouble(engComentarios + engCurtidas) / Convert.ToDouble(objMinhasMidias.Count());
                var mediaPercent = (mediaEngaj / inf.Seguidores) * 100;

                inf.percentAvg = Math.Round(mediaPercent, 2);
                if (mediaPercent < valor0)
                {
                    inf.Aprovado = 0;
                }
                else if (mediaPercent < valor1)
                {
                    inf.Aprovado = 1;
                }
                else
                {
                    inf.Aprovado = 2;
                }

                inf.Powerful = Helper.CalculoPowerful(lstCities);//lstStoryGraphs);
                var calcPowerFul = (Convert.ToDouble(inf.Powerful) / Convert.ToDouble(inf.Seguidores) * 100);
                if (calcPowerFul < valor0)
                {
                    inf.Aprovado = 0;
                }
                else if (calcPowerFul < valor1)
                {
                    inf.Aprovado = 1;
                }
                else
                {
                    inf.Aprovado = 2;
                }
                #endregion

                #region Minha midias
                linhaerro = "Minha midias";
                var lstMidiasT = objMinhasMidias.ToList();
                var lstMinhasMidias = lstMidiasT.Where(r => r.caption != null)
                    //.Where(z => z.comments_count > 0).ToList()
                    .Where(r => (r.media_url != null || r.permalink != null)).ToList();

                var lstMidias = lstMinhasMidias
                    .Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = "" + x.caption.ToString(),
                        Used = Math.Round(Convert.ToDouble
                    (
                        Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                    ), 0),
                        UsedPerc = Math.Round(Convert.ToDouble
                    (
                            (
                                Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                            /
                                Convert.ToDouble(lstMinhasMidias.Count())
                            )
                    ) * 100, 4),
                        Reach = lstMinhasMidias.Where(c => c.caption == x.caption)
                        .Sum(s => Convert.ToInt32(s.comments_count) + s.like_count),
                        Engagemer = Math.Round(
                                Convert.ToDouble
                                (
                                    (Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                            .Sum(s => Convert.ToInt32(s.comments_count))
                                        ) +
                                        Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                                .Sum(s => Convert.ToInt32(s.like_count))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(inf.Seguidores)
                                ) * 100, 4),
                        DiffUsedEngag = 0,
                        Imagens = new List<string>()
                        {
                            (x.media_url==null)?x.permalink.ToString():x.media_url.ToString()
                        },
                        Impressions = x.Impressions,
                        Reachs = x.Reach,
                        Saveds = x.Saved,
                        Engagements = x.Engagement,
                        TimeStamp = x.timestamp
                    }).ToList();
                inf.LstInstaMidias = lstMidias.Take(40) //.DistinctBy(d => d.us.UserName)
                    .ToList();
                var mediaMidias = lstMinhasMidias.DistinctBy(d => d.id).Count();
                var mediaMidiasVideo = lstMinhasMidias.Where(w => w.media_type.Contains("VIDEO")).Count();
                var mediaMidiasImage = lstMinhasMidias.Where(w => !w.media_type.Contains("VIDEO")).Count();

                var percentmediaMidiasVideo = mediaMidias == 0 ? 0d : Convert.ToDouble((Convert.ToDouble(mediaMidiasVideo) / (mediaMidias) * 100));
                var percentmediaMidiasImage = mediaMidias == 0 ? 0d : Convert.ToDouble((Convert.ToDouble(mediaMidiasImage) / (mediaMidias) * 100));

                ViewBag.mediaMidiasVideo = percentmediaMidiasVideo;
                ViewBag.mediaMidiasImage = percentmediaMidiasImage;
                infComentariosTotal = lstMinhasMidias.Sum(s => s.comments_count);
                #endregion

                #region Mentions
                linhaerro = "Hashtags";
                if (lstMongoMentions.Count > 0)
                {
                    var lstMentions = lstMongoMentions.Select(s => new Models.InstaMediaHash()
                    {
                        Hashs = Helper.SplitMentions(s.caption.ToUpper()),
                        InstaMedia = new InstaMedia()
                        {
                            Images = new List<InstaImage>() {
                                new InstaImage()
                                {
                                    URI = s.media_url
                                }
                            },
                            CommentsCount = s.comments_count.ToString(),
                            LikesCount = s.like_count
                        },
                        Impressions = s.Impressions,
                        Reachs = s.Reach,
                        Saveds = s.Saved,
                        Engagement = s.Engagement
                    }).ToList();
                    List<string> mentions = new List<string>();
                    lstMentions.ForEach(s =>
                    {
                        s.Hashs.ForEach(f =>
                        {
                            mentions.Add(f);
                        });
                    });
                    var lstImagensEmentions = new List<DtoHash>();
                    foreach (var it in lstMentions)
                    {
                        foreach (var h in it.Hashs)
                        {
                            lstImagensEmentions.Add(
                            new DtoHash()
                            {
                                hash = h,
                                URIImagem = (it.InstaMedia.Images.Count > 0 ? it.InstaMedia.Images.FirstOrDefault().URI : "")
                            });
                        }
                    }
                    var lstMentionsDist = mentions.DistinctBy(x => x).ToList();
                    var lstMentionsFinal = lstMentionsDist.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x
                        ,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstMentions.Where(u => u.Hashs.Contains(x)).Count())
                                /
                                    Convert.ToDouble(mentions.Count())
                                )
                        ) * 100, 4),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstMentions.Where(u => u.Hashs.Contains(x)).Count())

                        ), 0),
                        Reach = lstMentions.Where(c => c.Hashs.Contains(x))
                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount) + s.InstaMedia.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstMentions.Where(c => c.Hashs.Contains(x))
                                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstMentions.Where(c => c.Hashs.Contains(x))
                                                .Sum(s => Convert.ToInt32(s.InstaMedia.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstMentions.Where(c => c.Hashs.Contains(x))
                                        .Sum(s => mentions.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = Convert.ToInt32((Convert.ToDouble(lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Engagement))/Convert.ToDouble((inf.Seguidores))*100)),
                        Imagens =
                                        lstImagensEmentions
                                        .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                        .Select(s => s.URIImagem).ToList(),
                        Impressions = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Impressions),
                        Reachs = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Reachs),
                        Saveds = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Saveds),
                        Engagements = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Engagement),
                    }).ToList();
                    inf.LstInstaMentions = lstMentionsFinal.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region HashTags
                linhaerro = "Hashtags";
                if (lstMinhasMidias.Count > 0)
                {
                    var objHash = lstMinhasMidias.Where(w => w.caption.Contains("#"));
                    var lstHash = objHash.Where(x => x.comments != null).ToList();
                    var newLstHash = lstHash.Select(s => new Models.InstaMediaHash
                    {
                        Hashs = Helper.SplitHash(s.caption.ToUpper()),
                        InstaMedia = new InstaMedia()
                        {
                            Images = new List<InstaImage>() {
                                new InstaImage()
                                {
                                    URI = s.media_url
                                }
                            },
                            CommentsCount = s.comments_count.ToString(),
                            LikesCount = s.like_count
                        },
                        Impressions = s.Impressions,
                        Reachs = s.Reach,
                        Saveds = s.Saved,
                        Engagement = s.Engagement
                    }
                    ).ToList();

                    List<string> hashs = new List<string>();
                    newLstHash.ForEach(s =>
                    {
                        s.Hashs.ForEach(f =>
                        {
                            hashs.Add(f);
                        });
                    }
                    );

                    var lstImagensEhashs = new List<DtoHash>();
                    foreach (var it in newLstHash)
                    {
                        foreach (var h in it.Hashs)
                        {
                            lstImagensEhashs.Add(
                            new DtoHash()
                            {
                                hash = h,
                                URIImagem = (it.InstaMedia.Images.Count > 0 ? it.InstaMedia.Images.FirstOrDefault().URI : "")
                            });
                        }
                    }

                    var lstHashsDist = hashs.DistinctBy(x => x).ToList();
                    var lstHashs = lstHashsDist.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x
                        ,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())
                                /
                                    Convert.ToDouble(hashs.Count())
                                )
                        ) * 100, 4),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())

                        ), 0),
                        Reach = newLstHash.Where(c => c.Hashs.Contains(x))
                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount) + s.InstaMedia.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                                .Sum(s => Convert.ToInt32(s.InstaMedia.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        newLstHash.Where(c => c.Hashs.Contains(x))
                                        .Sum(s => hashs.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        lstImagensEhashs
                                        .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                        .Select(s => s.URIImagem).ToList(),
                        Impressions = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Impressions),
                        Reachs = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Reachs),
                        Saveds = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Saveds),
                        Engagements = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Engagement),
                    }).ToList();
                    inf.LstInstaHashs = lstHashs.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region TagsMentionedBy
                linhaerro = "MentionedByTag";
                if (mongoTagsUnico != null && (mongoTagsUnico.Count > 0))
                {
                    var lstTag = mongoTagsUnico.Where(x => x.caption != null).ToList();

                    var lstTags = lstTag.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x.username,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstTag.Where(u => u.username.Equals(x.username)).Count())
                                /
                                    Convert.ToDouble(lstTag.Count())
                                )
                        ) * 100, 4),
                        Used = 1/*Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstTag.Where(u => u.username.Equals(x.username)).Count())

                        ), 0)*/,
                        Reach = x.comments_count + x.like_count,
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstTag.Where(u => u.username.Equals(x.username))
                                            .Sum(s => Convert.ToInt32(s.comments_count))
                                        ) +
                                        Convert.ToDouble(
                                            lstTag.Where(u => u.username.Equals(x.username))
                                                .Sum(s => Convert.ToInt32(s.like_count))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstTag.Where(u => u.username.Equals(x.username))
                                        .Sum(s => lstTag.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        new List<string>(){
                                            x.media_url
                                        }
                    }).ToList();
                    inf.LstInstaTags = lstTags.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region Ages e Gender
                linhaerro = "Gender and Ages";
                if (lstAge != null)
                {
                    var lstAgesGender = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new
                        {
                            Gender = x.name.Split('.').GetValue(0),
                            Faixa = x.name.Split('.').GetValue(1),
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = 0,
                            DiffUsedEngag = 0
                        }).ToList();

                    var lstAgesMidias = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = "" + x.name,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = Convert.ToDouble(x.valor) / Convert.ToDouble(metricas.Cabecalho.Seguidores) * 100,
                            DiffUsedEngag = 0,
                            Imagens = null
                        }).ToList();
                    inf.LstAge = lstAgesMidias.OrderByDescending(o => o.Reach).Take(40) //.DistinctBy(d => d.us.UserName)
                        .ToList();

                    var listaArrayM = "";
                    var somaM = 0;
                    lstAgesMidias.ForEach(f =>
                    {
                        if (f.UserName.IndexOf("M") > -1)
                        {
                            listaArrayM += (f.Reach * -1).ToString() + ",";
                            somaM += f.Reach;
                        }
                    });
                    var listaArrayF = "";
                    var somaF = 0;
                    lstAgesMidias.ForEach(f =>
                    {
                        if (f.UserName.IndexOf("F") > -1)
                        {
                            listaArrayF += f.Reach.ToString() + ",";
                            somaF += f.Reach;
                        }
                    });
                    ViewBag.listaArrayM = listaArrayM;
                    ViewBag.listaArrayF = listaArrayF;
                    ViewBag.PercentM = Math.Round((Convert.ToDouble(somaM) / Convert.ToDouble(Convert.ToInt32(objUser.followers_count))) * 100, 2);
                    ViewBag.PercentF = Math.Round((Convert.ToDouble(somaF) / Convert.ToDouble(Convert.ToInt32(objUser.followers_count))) * 100, 2);
                }
                #endregion

                #region Cities
                linhaerro = "Cities";
                if (lstObjCities != null && lstObjCities.Count() > 0 && lstObjCities.FirstOrDefault().data.Count > 0)
                {
                    var lstCitiesResult = lstObjCities.FirstOrDefault().data
                        .Select(x => new
                        {
                            City = x.Key,
                            Number = x.Value,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = 0,
                            Engagemer = 0,
                            DiffUsedEngag = 0
                        }).ToList();

                    var lstCitiesResults = lstObjCities.FirstOrDefault().data
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = "" + x.Key,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.Value),
                            Engagemer = Convert.ToDouble(x.Value) / Convert.ToDouble(metricas.Cabecalho.Seguidores) * 100,
                            DiffUsedEngag = 0,
                            Imagens = null
                        }).ToList();
                    inf.LstCities = lstCitiesResults.OrderByDescending(o => o.Reach).Take(20) //.DistinctBy(d => d.us.UserName)
                        .ToList();
                    string lstCitiesArray = "[['City', 'Engagement'],";
                    inf.LstCities.ForEach(f =>
                    {
                        var city = f.UserName.Split(',')[0];
                        city = city == null ? f.UserName : city;
                        lstCitiesArray += "['" + city + "'," + f.Reach.ToString() + "],";
                    });
                    lstCitiesArray += "]";

                    var lstEstadosRegioes = lstCitiesResults.Select(s =>
                    new
                    {
                        Username = s.UserName.Replace("(state)", ""),
                        UF = Helper.GetUF(s.UserName),
                        Engagement = s.Engagemer,
                        Regiao = Helper.GetRegiao(s.UserName)
                    }).ToList();

                    var result = lstEstadosRegioes
                        .GroupBy(x => x.Regiao)
                        .Select(g => new
                        {
                            Total = g.Sum(x => x.Engagement > 0 ? x.Engagement : 0),
                            Regiao = g.First().Regiao
                        }).ToList();

                    #region Regiões
                    var topN = lstEstadosRegioes.Where(e => e.Regiao == "N")
                                                .OrderByDescending(o => o.Engagement)
                                                .ToList();
                    ViewBag.RegionN = "<li data-prefix='1º - '>" + (topN.Count > 0 ? topN.FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='2º - '>" + (topN.Count > 1 ? topN.Skip(1).FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='3º - '>" + (topN.Count > 2 ? topN.Skip(2).FirstOrDefault().Username : "") + "</li>";
                    var topNE = lstEstadosRegioes.Where(e => e.Regiao == "NE")
                                                .OrderByDescending(o => o.Engagement)
                                                .ToList();
                    ViewBag.RegionNE = "<li data-prefix='1º - '>" + (topNE.Count > 0 ? topNE.FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='2º - '>" + (topNE.Count > 1 ? topNE.Skip(1).FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='3º - '>" + (topNE.Count > 2 ? topNE.Skip(2).FirstOrDefault().Username : "") + "</li>";

                    var topS = lstEstadosRegioes.Where(e => e.Regiao == "S")
                                                .OrderByDescending(o => o.Engagement)
                                                .ToList();
                    ViewBag.RegionS = "<li data-prefix='1º - '>" + (topS.Count > 0 ? topS.FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='2º - '>" + (topS.Count > 1 ? topS.Skip(1).FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='3º - '>" + (topS.Count > 2 ? topS.Skip(2).FirstOrDefault().Username : "") + "</li>";

                    var topSE = lstEstadosRegioes.Where(e => e.Regiao == "SE")
                                                .OrderByDescending(o => o.Engagement)
                                                .ToList();
                    ViewBag.RegionSE = "<li data-prefix='1º - '>" + (topSE.Count > 0 ? topSE.FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='2º - '>" + (topSE.Count > 1 ? topSE.Skip(1).FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='3º - '>" + (topSE.Count > 2 ? topSE.Skip(2).FirstOrDefault().Username : "") + "</li>";

                    var topC = lstEstadosRegioes.Where(e => e.Regiao == "CE")
                                                .OrderByDescending(o => o.Engagement)
                                                .ToList();
                    ViewBag.RegionC = "<li data-prefix='1º - '>" + (topC.Count > 0 ? topC.FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='2º - '>" + (topC.Count > 1 ? topC.Skip(1).FirstOrDefault().Username : "") + "</li>" +
                                      "<li data-prefix='3º - '>" + (topC.Count > 2 ? topC.Skip(2).FirstOrDefault().Username : "") + "</li>";
                    var sumTopN = topN.Sum(s => s.Engagement);
                    var sumTopNE = topNE.Sum(s => s.Engagement);
                    var sumTopS = topS.Sum(s => s.Engagement);
                    var sumTopSE = topSE.Sum(s => s.Engagement);
                    var sumTopC = topC.Sum(s => s.Engagement);

                    var totalRegions = (sumTopC + sumTopN + sumTopNE + sumTopS + sumTopSE);
                    ViewBag.percN = Math.Round(sumTopN / totalRegions * 100, 1);
                    ViewBag.percNE = Math.Round(sumTopNE / totalRegions * 100, 1);
                    ViewBag.percS = Math.Round(sumTopS / totalRegions * 100, 1);
                    ViewBag.percSE = Math.Round(sumTopSE / totalRegions * 100, 1);
                    ViewBag.percC = Math.Round(sumTopC / totalRegions * 100, 1);
                    #endregion

                    lstCitiesArray = lstCitiesArray.Replace("],]", "]]");
                    ViewBag.LstCitiesArray = lstCitiesArray;
                    var percCities = Math.Round((Convert.ToDouble(inf.LstCities.FirstOrDefault().Reach) / Convert.ToDouble(Convert.ToInt32(objUser.followers_count))) * 100, 2);
                    ViewBag.PercentCities = percCities.ToString() + "% - " + inf.LstCities.FirstOrDefault().UserName;
                    //var latitudes = await Coordenates(lstCitiesResults.FirstOrDefault().UserName);
                }
                #endregion

                #region Top and Botton POST
                if (inf.LstInstaMidias.Count() > 0)
                {
                    inf.LstTopAndBotton = inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Max(m => m.Engagemer)).ToList();
                }
                #endregion

                #region Stories
                linhaerro = "Stories";

                var lstGraphsStory = lstStoryGraphs
                    .Where(w=>w.Obj != null)
                    .Where(d=>d.Obj.data != null)
                    .Select(s => new Models.DTO.Story()
                {
                    ImpressionsValue = s.Obj.data.Where(w => w.name == "impressions").FirstOrDefault().values.FirstOrDefault().value,
                    ReachValue = s.Obj.data.Where(w => w.name == "reach").FirstOrDefault().values.FirstOrDefault().value,
                    ExitsValue = s.Obj.data.Where(w => w.name == "exits").FirstOrDefault().values.FirstOrDefault().value,
                    RepliesValue = s.Obj.data.Where(w => w.name == "replies").FirstOrDefault().values.FirstOrDefault().value,
                    TapsForwardValue = s.Obj.data.Where(w => w.name == "taps_forward").FirstOrDefault().values.FirstOrDefault().value,
                    TapsBackValue = s.Obj.data.Where(w => w.name == "taps_back").FirstOrDefault().values.FirstOrDefault().value,
                    DateCreation = s.DateCreation,
                    Id = (s.Obj.data.FirstOrDefault().id.Split('/')[0]).ToString().ToUpper(),
                    TimeSpan = s.timeSpan
                    //(lstStoryGraphs.Where(lw => lw.Obj.data.FirstOrDefault().id.Split('/')[0] ==
                    //                    s.Obj.data.FirstOrDefault().id.Split('/')[0] &&
                    //                    s.DateCreation == lw.DateCreation).Max(mt => mt.timeSpan))
                });

                var lstTeste = lstGraphsStory.ToList();

                var lstGraphsStoryIds = lstGraphsStory
                    .GroupBy(g => new { g.Id })
                    .Select(s => new
                    {
                        Id = s.Key.ToString().ToUpper(),
                        TimeSpanMax = s.Max(ma => ma.TimeSpan)
                    });

                var lstStory = new List<Models.DTO.Story>();
                foreach (var it in lstGraphsStory.DistinctBy(d => d.Id))
                {
                    var _maxTime = lstGraphsStory.Where(w => w.Id.ToString().Equals(it.Id)).ToList();
                    var maxTime = _maxTime.Max(m => m.TimeSpan);
                    var newStory = lstGraphsStory.Where(w => w.Id == it.Id && w.TimeSpan == maxTime).FirstOrDefault();
                    lstStory.Add(new Story()
                    {
                        DateCreation = newStory.DateCreation,
                        Date = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(newStory.TimeSpan)).ToString("dd/MM/yyyy hh:mm"), // lstStoryIdsTmp.Where(w=>w.Obj newStory.DateCreation.ToString("dd/MM/yyyy"),
                        Id = newStory.Id,
                        TimeSpan = newStory.TimeSpan,
                        ExitsValue = newStory.ExitsValue,
                        ImpressionsValue = newStory.ImpressionsValue,
                        ReachValue = newStory.ReachValue,
                        RepliesValue = newStory.RepliesValue,
                        TapsBackValue = newStory.TapsBackValue,
                        TapsForwardValue = newStory.TapsForwardValue,
                        Imagens =
                                new List<string>(){
                                    //"https://www.influencersmetrics.com/story/image/" +
                                    "https://s3.amazonaws.com/influencersmetrics/" +
                                        (_lstStoryIds.Exists(w=>w.Contains(newStory.Id) && w.Contains("IMAGE"))? newStory.Id:"VIDEO") + ".jpg"
                                }
                    });
                }

                var lstGraphStoriesI = lstStory.Select(s => new
                {
                    Name = "Impressions",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ImpressionsValue
                }).Take(10);

                var lstGraphStoriesR = lstStory.Select(s => new
                {
                    Name = "Reach",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ReachValue
                }).Take(10);

                var lstGraphStoriesE = lstStory.Select(s => new
                {
                    Name = "Exits",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ExitsValue
                }).Take(10);

                var lstGraphStoriesRp = lstStory.Select(s => new
                {
                    Name = "Replies",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.RepliesValue
                }).Take(10);

                var lstGraphStoriesTf = lstStory.Select(s => new
                {
                    Name = "Taps forward",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.TapsForwardValue
                }).Take(10);

                var lstGraphStoriesTb = lstStory.Select(s => new
                {
                    Name = "Taps back",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.TapsBackValue
                }).Take(10);

                var lstGraphStories = lstGraphStoriesI.Union(lstGraphStoriesR).Union(lstGraphStoriesE)
                                      .Union(lstGraphStoriesRp).Union(lstGraphStoriesTf).Union(lstGraphStoriesTb);
                if (lstGraphStories.Count() > 0)
                {
                    var lstStories = lstGraphStories.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x.Name,
                        UsedPerc = 0,
                        Used = 1,
                        Reach = Convert.ToInt32((
                                lstGraphStories.Where(w => w.Id == x.Id && x.Name == w.Name) //dtCriacao.AddDays(1)
                                                .Max(z => z.Avg))
                                                ),
                        Engagemer = 0,
                        DiffUsedEngag = 1,
                        Imagens =
                                        new List<string>(){
                                            //"https://www.influencersmetrics.com/story/image/" +
                                            "https://s3.amazonaws.com/influencersmetrics/" +
                                                (_lstStoryIds.Exists(w=>w.Contains(x.Id) && w.Contains("IMAGE"))? x.Id:"VIDEO") + ".jpg"
                                        }
                    });

                    var mediaStory = lstGraphsStory.DistinctBy(d => d.Id).Count();
                    var dataMaxStory = lstGraphsStory.Max(m => m.DateCreation);
                    var dataMinStory = lstGraphsStory.Min(m => m.DateCreation);
                    var diffMaxMin = (dataMaxStory.Date - dataMinStory.Date).Days;
                    mediaStory = mediaStory / (diffMaxMin == 0 ? 1 : diffMaxMin);
                    ViewBag.MediaStories = mediaStory;

                    mediaStoryVideo = lstStory.Where(w => w.Imagens.Exists(e => e.Contains("VIDEO.jpg"))).DistinctBy(d => d.Id).Count();
                    mediaStoryImage = lstStory.Where(w => w.Imagens.Exists(e => !e.Contains("VIDEO.jpg"))).DistinctBy(d => d.Id).Count();

                    percentmediaStoryVideo = Convert.ToDouble((Convert.ToDouble(mediaStoryVideo) / (lstStory.DistinctBy(d => d.Id).Count()) * 100));
                    percentmediaStoryImage = Convert.ToDouble((Convert.ToDouble(mediaStoryImage) / (lstStory.DistinctBy(d => d.Id).Count()) * 100));

                    inf.LstInstaStories = lstStories.Take(60).ToList();
                    inf.LstInstaStory = lstStory.OrderByDescending(x => x.DateCreation).ToList();
                    infRepliesTotal = lstStory.Sum(s => s.RepliesValue);
                }

                #endregion

                #region Emotional
                linhaerro = "Emotional";
                if (lstFaceDetection.Count > 0)
                {
                    var lstSemNota = lstFaceDetection.FirstOrDefault().Obj
                        .Where(w => w.Joy == 0 && w.Anger == 0 && w.Surprise == 0 && w.Sorrow == 0)
                        .Select(s => s.UrlImagem).ToList();

                    var ls = from s in lstFaceDetection.FirstOrDefault().Obj
                             where !lstSemNota.Any(es => (es == s.UrlImagem))
                             select s;

                    var lstFaceDetections = ls
                        .Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger), //Raiva
                            Joy = (s.Joy), //Alegria
                            Sorrow = (s.Sorrow), //Tristeza
                            Surprise = (s.Surprise), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                    var lstAvgFaceDetection = lstFaceDetections.Select(l => new Models.FaceDetection()
                    {
                        Anger = lstFaceDetections.Sum(s => (s.Anger)), //Raiva
                        Joy = lstFaceDetections.Sum(s => (s.Joy)), //Alegria
                        Sorrow = lstFaceDetections.Sum(s => s.Sorrow), //Tristeza
                        Surprise = lstFaceDetections.Sum(s => (s.Surprise)), //Surpresa
                        DtAvaliacao = lstFaceDetections.FirstOrDefault().DtAvaliacao,
                        UrlImagem = "",
                        UserName = ""
                    });
                    var avgFaceDetection = lstAvgFaceDetection.DistinctBy(d => d.UserName).FirstOrDefault();
                    if (avgFaceDetection == null)
                    {

                    }
                    else
                    {
                        inf.LstFaceDetection = lstFaceDetections.ToList();
                        inf.AvgFaceDetection = avgFaceDetection;

                        var listaFaceDetection = (avgFaceDetection.Joy / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Sorrow / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Anger / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Surprise / lstFaceDetection.Count).ToString() + "";
                        var cabecalhoFaceDetection = "'Alegre','Tristeza','Raiva','Surpresa'";

                        ViewBag.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        ViewBag.ListaFaceDetection = listaFaceDetection;

                        inf.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        inf.ListaFaceDetection = listaFaceDetection;

                        var lstEmotionals = ls.Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger), //Raiva
                            Joy = (s.Joy), //Alegria
                            Sorrow = (s.Sorrow), //Tristeza
                            Surprise = (s.Surprise), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                        var newLstEmotional = lstEmotionals.Select(s => new
                        {
                            Joy = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Joy),
                            Sorrow = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Sorrow),
                            Anger = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Anger),
                            Surprise = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Surprise)
                        }
                        ).ToList().Distinct();

                        inf.LstInstaEmotionalResume = new List<InstaMentions>();

                        newLstEmotional.ForEach(f =>
                        {
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "ALEGRIA",
                                Reach = f.Joy
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "TRISTEZA",
                                Reach = f.Sorrow
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "RAIVA",
                                Reach = f.Anger
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "SURPRESA",
                                Reach = f.Surprise
                            });
                        });

                        var strEmotionalNumeros = "";
                        var totalEmotional = inf.LstInstaEmotionalResume.Sum(s => s.Reach);
                        inf.LstInstaEmotionalResume.ForEach(f =>
                        {
                            f.Engagemer = Convert.ToDouble(((f.Reach / totalEmotional) * 100));
                            strEmotionalNumeros += f.Reach.ToString() + ",";
                        });

                        ViewBag.EmotionalResume = strEmotionalNumeros;
                    }
                }
                #endregion

                ViewBag.mediaStoryVideo = percentmediaStoryVideo;
                ViewBag.mediaStoryImage = percentmediaStoryImage;

                ViewBag.infRepliesTotal = infRepliesTotal;
                ViewBag.infComentariosTotal = infComentariosTotal;

                metricas.inf = inf;

                HttpContext.Session.SetString("ProfilePictureMidiakit", HttpUtility.UrlDecode(inf.ProfilePicture));
                HttpContext.Session.SetString("NomeCompleto", inf.NomeCompleto);
                HttpContext.Session.SetString("UserName", inf.UserName);
                HttpContext.Session.SetString("SocialContext", inf.SocialContext.Replace("\n", " "));
                //await repMongo.GravarOne<Models.DTO.InfluencersResumoFree>(inf);

                var pp = HttpContext.Session.GetString("ProfilePictureMidiakit");
                var nc = HttpContext.Session.GetString("NomeCompleto");
                var un = HttpContext.Session.GetString("UserName");
                var sc = HttpContext.Session.GetString("SocialContext");
                ViewBag.ProfilePicture = pp;
                ViewBag.NomeCompleto = nc;
                ViewBag.UserName = un;
                ViewBag.SocialContext = sc;

                return View(metricas);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Erro inesperado a processar a visualização.<br />Por favor tent novamente em alguns minutos (" + linhaerro + ")";
                return View(new Models.Metricas());
            }
        }
        public IActionResult Pesquisas()
        {
            return View();
        }
        public IActionResult PesquisasPlanos()
        {
            return View();
        }
        public IActionResult PlanosAgencias()
        {
            return View();
        }
        public IActionResult PlanosInfluenciadores()
        {
            return View();
        }
        public IActionResult SocialStalker()
        {
            return View();
        }
        public IActionResult SocialStalkerResult()
        {
            return View();
        }
        //public IActionResult SolicitacaoMetrica()
        //{
        //    return View();
        //}
    }
}