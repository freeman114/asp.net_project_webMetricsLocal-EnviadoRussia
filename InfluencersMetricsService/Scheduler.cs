using InfluencersMetricsService.Helper;
using InfluencersMetricsService.Model;
using Microsoft.Extensions.Options;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using webMetrics.Models;

namespace InfluencersMetricsService
{
    public partial class Scheduler : ServiceBase
    {
        #region Variaveis
        public static List<Model.Suspensao> access_token_suspensos = new List<Model.Suspensao>();
        public static List<string> media_error = new List<string>();
        private static int ContadorTempo { get; set; }
        private static int ContadorCorrecoes { get; set; }
        private static int ContadorDia { get; set; }
        private Timer timer1 = null;
        private bool processadodiario { get; set; }
        private DateTime DataUltimaLimpeza = new DateTime();
        private static IOptions<AppSettings> _settings;
        HttpClient client = new HttpClient();
        #endregion

        public Scheduler()
        {
            InitializeComponent();
            ContadorTempo = 5;
            ContadorCorrecoes = 60;
            ContadorDia = 0;

            //MakeUserBestHourAsync().Wait();
           // MakeStories();

            //MakeInfluencersInc().Wait();

            MakeMedias().Wait();
            //Make();

            //MakePayments().Wait();
            //MakeInvoices();
            //executeCorrecoes();
            //            executeCorrecoes();

            //MakeFiles();
            //UploadFile.UploadFileLoad(@"C:\Users\Samsung i5\Pictures\TESTE.MP4", "teste.MP4");
            //MakeMedias().Wait();
            //MakeStories();
            //Limpeza();
        }

        #region Configurações
        protected override void OnStart(string[] args)
        {
            timer1 = new Timer
            {
                Interval = 120000
            };
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Tick);
            timer1.Enabled = true;
        }

        protected override void OnStop()
        {
            timer1.Enabled = false;
            Library.WriteErrorLog("InfluencersMetricsService stopped");
        }

        public void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            ContadorTempo++;
            ContadorCorrecoes++;
            if (!Debugger.IsAttached)
            {
                timer1.Stop();
            }
            Library.WriteErrorLog("Timer_Ticker(Stop) Inicio de processamento... (" + ContadorTempo.ToString() + ")");

            Make();

            Limpeza();

            if (!Debugger.IsAttached)
            {
                timer1.Start();
            }
            Library.WriteErrorLog("Timer_Ticker(Start) Fim de processamento...");
        }

        private static void setSetting()
        {
            if (_settings == null)
            {
                _settings = Options.Create(new webMetrics.Models.AppSettings()
                {
                    ConexaoMongoDB = "mongodb://myuserMetrics:28111981@168.235.111.153:27018"
                });
            }
        }

        private static long ConvertToTimestamp(DateTime value)
        {
            long epoch = (value.Ticks - 621355968000000000) / 10000000;
            return epoch;
        }

        private void MakeFiles()
        {
            DirectoryInfo diretorio = new DirectoryInfo(@"C:\output");
            FileInfo[] Arquivos = diretorio.GetFiles("*.*");

            foreach (FileInfo fileinfo in Arquivos)
            {
                string t = fileinfo.Name;
                var strReturn = UploadFile.UploadFileLoad(fileinfo.FullName, t);
            }
        }

        private async Task Limpeza()
        {
            if (DataUltimaLimpeza.Date != DateTime.Now.Date)
            {
                Library.WriteErrorLog("Inicio de limpeza");
                DataUltimaLimpeza = DateTime.Now; string source = @"c:\output\";
                if (System.IO.Directory.Exists(source))
                {
                    string[] files = System.IO.Directory.GetFiles(source);
                    int contFiles = 0;
                    foreach (string s in files)
                    {
                        string fileName = System.IO.Path.GetFileName(s);
                        if (fileName.Length > 39)
                        {
                            System.IO.File.Delete(s);
                            contFiles++;
                        }
                    }
                    Library.WriteErrorLog("Final de limpeza, total de arquivos excluidos: " + contFiles.ToString());
                }
            }
        }
        #endregion

        private void Make()
        {
            var OqueFaco = "Start";
            try
            {
                if (DateTime.Now.Hour == 2 && processadodiario == false)
                {
                    processadodiario = true;
                    Library.WriteErrorLog("Processamento - Diário - Start");
                    MakeDiarios();
                    Library.WriteErrorLog("Processamento - Diário - Final");

                    if (ContadorTempo == 6) //6 minutos para ter 10 leituras por hora
                    {
                        ContadorTempo = 0;
                    }
                    return;
                }
                processadodiario = (DateTime.Now.Hour == 2)? processadodiario: false;

                OqueFaco = "Payments";
                Library.WriteErrorLog(OqueFaco + " - Start");
                MakePayments().Wait();
                Library.WriteErrorLog(OqueFaco + " - Final");


                if (ContadorTempo == 6) //6 minutos para ter 10 leituras por hora
                {
                    ContadorTempo = 0;
                    OqueFaco = "Stories";
                    Library.WriteErrorLog(OqueFaco + " - Start");
                    MakeStories(); //ok
                    Library.WriteErrorLog(OqueFaco + " - Finish");
                }


                OqueFaco = "Insigths";
                Library.WriteErrorLog(OqueFaco + " - Start");
                MakeInsights();
                Library.WriteErrorLog(OqueFaco + " - Finish");

                OqueFaco = "Medias";
                Library.WriteErrorLog(OqueFaco + " - Start");
                MakeMedias().Wait(); //ok
                Library.WriteErrorLog(OqueFaco + " - Finish");
                
                OqueFaco = "Importação - INC";
                Library.WriteErrorLog(OqueFaco + " - Start");
                MakeInfluencersInc().Wait();
                Library.WriteErrorLog(OqueFaco + " - Finish");


                OqueFaco = "RemovendoTokens";
                //Limpando tokens suspensos
                access_token_suspensos.RemoveAll(r => r.DtExpirou <= DateTime.Now);

                if (ContadorCorrecoes == 60)
                {
                    OqueFaco = "Correções";
                    ContadorCorrecoes = 0;
                    //Library.WriteErrorLog("MakeCorreções: " + OqueFaco);
                    //MakeCorrecoes();
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog("Erro:: Make: (" + OqueFaco + ") " + ex.Message);
            }
        }

        private async Task MakeDiarios()
        {
            var listTokensUser = Library.GetDataAsync<List<Model.Response<string>>>().Result;
            if (listTokensUser != null)
            {
                setSetting();
                Library.WriteErrorLog("MakeDiarios: " + listTokensUser.Count());
                var listTokenUserDistinct = listTokensUser.Select(s => s.access_token).DistinctBy(d => d);
                var tskGrowTh = MakeGrowthProcessAsync(listTokensUser, listTokenUserDistinct);
                var tskBestHour = MakeBestHourAsync(listTokensUser, listTokenUserDistinct);

                Task.WaitAll(tskGrowTh, tskBestHour);

                await MakeUserBestHourAsync();
            }
        }

        private async Task MakeUserBestHourAsync()
        {
            var lst = new List<InfluencersMetricsService.Model.StoryBestHour>();
            setSetting();
            InfluencersMetrics.MongoRep repMongo = new InfluencersMetrics.MongoRep("ROBO", _settings);
            var lstStories = await repMongo.ListarStoriesBest();
            Library.WriteErrorLog("MakeDiarios - ListarStoriesBest");
            lstStories.ForEach(i =>
               {
                   i.Obj.ForEach(f =>
                   {
                       f.UsuarioId = i.UsuarioId;
                   });
                   i.Obj.ForEach(f =>
                   {
                       lst.Add(new StoryBestHour()
                       {
                           DateCreation = f.DateCreation,
                           DiaDaSemana = f.DiaDaSemana,
                           Hour = f.Hour,
                           idStory = f.idStory,
                           UsuarioId = f.UsuarioId,
                           ValorReach = f.ValorReach
                       });
                   });
               });

            Library.WriteErrorLog("MakeDiarios - lstFull - Antes");

            var lstFull = lst
           .GroupBy(a => new { a.UsuarioId, a.DiaDaSemana, a.idStory, a.Hour })
           .Select(a => new InfluencersMetricsService.Model.StoryUserBestHour()
           {
               UsuarioId = a.First().UsuarioId,
               DiaDaSemana = a.First().DiaDaSemana,
               Hour = a.First().Hour,
               ValorReach = a.Max(m => m.ValorReach) - a.Min(m => m.ValorReach)
           }
           ).ToList();
            Library.WriteErrorLog("MakeDiarios - lstFull - Depois");

            List<Task> lstTasks = new List<Task>();
            lstFull.Select(s => s.UsuarioId).Distinct().ForEach(us =>
            {
                repMongo = new InfluencersMetrics.MongoRep("ROBO", _settings, us);
                lstTasks.Add(repMongo.GravarOne<List<StoryUserBestHour>>(lstFull.Where(w => w.UsuarioId == us).ToList()));
            });
            Library.WriteErrorLog("MakeDiarios - foreach repMongo: Task==>" + lstTasks.Count().ToString());
            Task.WaitAll(lstTasks.ToArray());
            Library.WriteErrorLog("MakeDiarios - foreach repMongo Pronto");

            //var lstUsuario = lst.Select(s => s.UsuarioId).Distinct();
            //foreach (var us in lstUsuario)
            //{
            //    var lstDiaSemana = lst.Where(w => w.UsuarioId == us).Select(s => s.DiaDaSemana).Distinct();
            //    foreach (var ds in lstDiaSemana)
            //    {
            //        var lstStory = lst.Where(w => w.UsuarioId == us && w.DiaDaSemana == ds).Select(s => s.idStory).Distinct();
            //        foreach (var stor in lstStory)
            //        {
            //            var lstHour = lst.Where(w => w.UsuarioId == us && w.DiaDaSemana == ds && w.idStory == stor).Select(s => s.Hour).Distinct();
            //            foreach (var hr in lstHour)
            //            {
            //                var lstItens = lst.Where(w => w.UsuarioId == us && w.DiaDaSemana == ds && w.idStory == stor && w.Hour == hr);
            //                var max = lstItens.Where(w => w.UsuarioId == us && w.DiaDaSemana == ds && w.idStory == stor && w.Hour == hr).Max(m => m.ValorReach);
            //                var min = lstItens.Where(w => w.UsuarioId == us && w.DiaDaSemana == ds && w.idStory == stor && w.Hour == hr).Min(m => m.ValorReach);

            //                var diff = max - min;
            //                lstUserBestHour.Add(new StoryUserBestHour()
            //                {
            //                    UsuarioId = us,
            //                    DiaDaSemana = ds,
            //                    Hour = hr,
            //                    ValorReach = diff
            //                });
            //            }
            //        }
            //    }

            //    repMongo = new InfluencersMetrics.MongoRep("ROBO", _settings, us);
            //    await repMongo.GravarOne<List<StoryUserBestHour>>(lstUserBestHour);
            //    lstUserBestHour = new List<StoryUserBestHour>();
            //}
        }

        private async Task MakeBestHourAsync(List<Response<string>> listTokensUser, IEnumerable<string> listTokenUserDistinct)
        {
            DateTime dtInicio = DateTime.Now;
            setSetting();
            InfluencersMetrics.MongoRep repMongo = new InfluencersMetrics.MongoRep("ROBO", _settings);
            await repMongo.LimparBestHour();
            var lstInsightsFull = await repMongo.ListarGraphStoryId();

            foreach (var _access_token in listTokenUserDistinct)
            {
                try
                {
                    if (!access_token_suspensos.Exists(e => e.AccessToken == _access_token))
                    {
                        var lstUsers = listTokensUser.Where(w => w.access_token == _access_token).Select(s => s.userId).ToList();
                        var lstStories = await repMongo.GetStoriesPending(lstUsers);
                        var lstStoryBestHour = new List<InfluencersMetricsService.Model.StoryBestHour>();
                        var lstStoryBestHourC = new ConcurrentBag<InfluencersMetricsService.Model.StoryBestHour>();
                        var lstStoryId = new List<string>();
                        var usuarioIdFirst = listTokensUser.Where(w => w.access_token == _access_token).FirstOrDefault().userId;
                        var lstInsightsByUser = lstInsightsFull.Where(e => e.Obj != null && ((e.Obj.data != null) && (lstUsers.Contains(e.UsuarioId)))).ToList();
                        try
                        {
                            foreach (var itemStory in lstStories)
                            {
                                if (itemStory.Obj != null && (itemStory.Obj.data != null))
                                {
                                    itemStory.Obj.data.ForEach(s =>
                                    {
                                        lstStoryId.Add(s.id.Replace("/insights/reach/lifetime", "").ToString());
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        var lstStoryIdEnumerable = lstStoryId.Distinct();
                        try
                        {
                            var lstInsights = lstInsightsByUser.Where(e => e.Obj != null && ((e.Obj.data != null) &&
                                (e.Obj.data.Exists(x => lstStoryIdEnumerable.Select(ds => ds + "/insights/reach/lifetime").ToList().Contains(x.id))))).ToList();

                            Parallel.ForEach(lstInsights, (f) =>
                            //lstInsights.AsParallel(f =>
                            {
                                if (f.Obj != null && (f.Obj.data != null))
                                {
                                    var hour = new InfluencersMetricsService.Model.StoryBestHour
                                    {
                                        UsuarioId = "",
                                        idStory = f.Obj.data.Where(w => w.name == "reach").FirstOrDefault().id.Replace("/insights/reach/lifetime", ""),
                                        DateCreation = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(f.timeSpan)),
                                        Hour = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(f.timeSpan)).Hour,
                                        ValorReach = f.Obj.data.Where(w => w.name == "reach").FirstOrDefault().values.FirstOrDefault().value,
                                        DiaDaSemana = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(f.timeSpan)).DayOfWeek
                                    };
                                    lstStoryBestHourC.Add(hour);
                                }
                            });
                            lstStoryBestHour = lstStoryBestHourC.Select(s => (StoryBestHour)(s)).ToList();
                        }
                        catch (Exception ex)
                        {
                            //break;
                        }

                        if (lstStoryBestHour.Count() > 0)
                        {
                            foreach (var _item in lstUsers)
                            {
                                await repMongo.GravarStoryBestHour<List<InfluencersMetricsService.Model.StoryBestHour>>(lstStoryBestHour, _item);
                            }

                            lstStoryBestHour = new List<StoryBestHour>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Gerar log
                }
            }
            DateTime dtFinal = DateTime.Now;
        }

        private async Task MakeGrowthProcessAsync(List<Response<string>> listTokensUser, IEnumerable<string> listTokenUserDistinct)
        {
            foreach (var _access_token in listTokenUserDistinct)
            {
                try
                {
                    var dtInicial = ConvertToTimestamp(DateTime.Now.AddDays(-31)); // TimeSpan.FromTicks(DateTime.Now.AddDays(-31).Ticks);
                    var dtFinal = ConvertToTimestamp(DateTime.Now.AddDays(-1));// TimeSpan.FromTicks(DateTime.Now.Ticks);

                    if (!access_token_suspensos.Exists(e => e.AccessToken == _access_token))
                    {
                        var resultGrowth = Library.GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>(_access_token,
                                listTokensUser.Where(w => w.access_token == _access_token).FirstOrDefault().Obj +
                                "/insights?until=" + dtFinal.ToString() + "&since=" + dtInicial.ToString() + "&period=day&metric=follower_count",
                                "", listTokensUser.Where(w => w.access_token == _access_token).Select(s => s.userId).ToList()).Result;
                        foreach (var _item in listTokensUser.Where(w => w.access_token == _access_token).ToList())
                        {
                            var resultGrowthData = await setJsonData(JsonConvert.SerializeObject(resultGrowth), "novaconsulta", _item.userId, "Growth");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Gerar log
                }
            }
        }

        private async void executeCorrecoes()
        {
            /////
            var access_token = "EAADIfWmjGnMBAIF7ZByg1tmooKvOewH1dbmxRCMZBaWrd6mfVU1oZCCQolosQcA9lPnXQ6XKFAyX9RCw62JkHJCeh27y4d0K3ZBY0nsUkZCWenjPXJzZBGKvwnisQm584zQnZAL9xuTRyKTG5GYGPjBBKOCCOxJOal3PTVd48oa7GpnSRNINfoi";
            var userId = "5d003d878daa4f14a457bc87";
            var media = Library.GetDataGraphAsync<webMetrics.Models.Graph.Media>(access_token,
                                            "17841400773196692/media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username", userId).Result;
            var resultMedia = JsonConvert.SerializeObject(media);

            var feito = await Library.SetMediaUserAsync(resultMedia, "Scheduler", userId, "Media");
            //media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username
        }

        private async Task MakeMedias()
        {
            string linhadoerro = "";
            try
            {
                linhadoerro = "Lista";
                //var ListTokenMedias = Library.GetListMediaInsightsAsync<List<InfluencersMetricsService.Model.Response<webMetrics.Models.Graph.Media>>>
                //    ().Result;
                var ListTokenMedias = await Library.GetListMediaInsightsPendingAsync<List<InfluencersMetricsService.Model.Response<webMetrics.Models.Graph.Media>>>(30);

                Library.WriteErrorLog("Medias: Processou");
                var impressions = 0;
                var reach = 0;
                var engagement = 0;
                var saved = 0;
                linhadoerro = "foreach";
                if (ListTokenMedias != null)
                {
                    Library.WriteErrorLog("Medias: " + ListTokenMedias.Count());
                    foreach (var item in ListTokenMedias) // Lista de todas as midias
                    {
                        if (!access_token_suspensos.Exists(e => e.AccessToken == item.access_token))
                        {
                            var lstURIs = new List<string>();
                            linhadoerro = "foreach==>" + item.access_token;
                            foreach (var itMedia in item.Obj.data)
                            {
                                linhadoerro = "Insights de cada midia";

                                #region Insights de cada midia
                                var media = Library.GetDataGraphAsync<Model.StoryInsights>(item.access_token,
                                            itMedia.id + "/insights?metric=reach,impressions,engagement,saved", item.userId).Result;
                                linhadoerro = "Media";
                                if (media != null && !itMedia.Insight && (media.data != null))
                                {
                                    foreach (var itInsight in media.data)//Todos os insights de cada midia
                                    {
                                        if (itInsight.name == "impressions")
                                        {
                                            impressions = itInsight.values[0].value;
                                        }
                                        if (itInsight.name == "reach")
                                        {
                                            reach = itInsight.values[0].value;
                                        }
                                        if (itInsight.name == "saved")
                                        {
                                            saved = itInsight.values[0].value;
                                        }
                                        if (itInsight.name == "engagement")
                                        {
                                            engagement = itInsight.values[0].value;
                                        }
                                    }
                                    if (itMedia.media_type == "IMAGE") lstURIs.Add(itMedia.media_url);//Só adiciono se tiver insigths
                                }
                                itMedia.Impressions = impressions;
                                itMedia.Reach = reach;
                                itMedia.Saved = saved;
                                itMedia.Engagement = engagement;
                                itMedia.Insight = true;

                                linhadoerro = "MediaInsight";
                                var feito = await Library.SetMediaInsightAsync<bool>(item.access_token, "setmediainsight", item.userId, itMedia.id,
                                    impressions, reach, saved, engagement, item.id);
                                #endregion                                
                            }

                            linhadoerro = "SetAnottation";
                            var result = await Library.SetAnottation<string>(lstURIs, item.userId);
                            if (!result)
                            {
                                Library.WriteErrorLog("MakeMedias::SetAnottation::UserId(" + item.userId + ")::TotalListaUrl(+" + lstURIs.Count().ToString() + ")::Lista:(" + JsonConvert.SerializeObject(lstURIs).ToString() + ")");
                            }
                            else
                            {
                                //Library.WriteErrorLog("SetAnottation: access_token(" + item.access_token + ")");
                            }
                        }
                    }
                }

                linhadoerro = "ListMedias";
                //Stories dos emotionals profiles - ListarMediasWithEmotionalByAgencia
                var ListMediasByAgencias = Library.GetListarMediasWithEmotionalByAgenciaAsync<List<MediaToken>>("").Result;
                if (ListMediasByAgencias != null)
                {
                    ListMediasByAgencias.ForEach(f =>
                    {
                        var lstURIs = f.data.ToList().Select(s => s.media_url).ToList();

                        var result = Library.SetAnottation<string>(lstURIs, f.IdDiscovery).Result;

                        var marcarGravada = Library.SetEmotionalDiscovery(f.IdDiscovery).Result;
                    });
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog("MakeMedia," + linhadoerro + "," + ex.Message.ToString());
            }
        }

        private void MakeInsights()
        {
            var ListTokensUser = Library.GetInsightUserAsync<List<Model.Response<string>>>().Result;

            if (ListTokensUser != null)
            {
                Library.WriteErrorLog("Insights User: " + ListTokensUser.Count());
                foreach (var itemUser in ListTokensUser)
                {
                    if (itemUser.userId != null)
                    {
                        var _access_token = itemUser.access_token; //"EAADIfWmjGnMBAGwcZB6nCSuLCyTBsAwMSnOzkJ9Po0Q7nAP8YBlZBW9WmGaaU2tS0Jdi3AEqb9mFT85MxBSFW0JUAMeDeCmO21laD9S3Y00ur61f8l4enxdsZAWHpZBA5uxVqrJYFGW2FGTJN7fHADVYCc5I1TYe85NNz5fBpQZDZD";
                        var _instagram_business_account = itemUser.instagram_business_account;//"17841405542018257";//userId
                        var _userId = itemUser.userId;//"952357081614016";
                        var obj = itemUser.Obj;

                        if (!access_token_suspensos.Exists(e => e.AccessToken == _access_token))
                        {
                            var item = Library.GetDataGraphAsync<Model.StoryInsights>(_access_token,
                                itemUser.instagram_business_account + "/insights?metric=impressions,reach&period=week", _userId).Result;

                            if (item != null)
                            {
                                var itens = Library.SetDataGraphAsync<Model.StoryInsights>(_access_token, "setDataUsersInsights", _userId, obj, item).Result;
                            }

                            item = Library.GetDataGraphAsync<Model.StoryInsights>(_access_token,
                                itemUser.instagram_business_account + "/insights?metric=profile_views&period=day", _userId).Result;

                            if (item != null)
                            {
                                var itens = Library.SetDataGraphAsync<Model.StoryInsights>(_access_token, "setDataUsersInsights", _userId, obj, item).Result;
                            }
                        }
                    }
                }
            }
        }

        private void MakeStories()
        {
            var listTokensUser = Library.GetDataAsync<List<Model.Response<string>>>().Result;
            if (listTokensUser != null)
            {
                Library.WriteErrorLog("Stories: " + listTokensUser.Count());
                var listTokenUserDistinct = listTokensUser.Select(s => s.access_token).DistinctBy(d => d);
                foreach (var _access_token in listTokenUserDistinct)
                {
                    //var _access_token = itemUser.access_token; //"EAADIfWmjGnMBAGwcZB6nCSuLCyTBsAwMSnOzkJ9Po0Q7nAP8YBlZBW9WmGaaU2tS0Jdi3AEqb9mFT85MxBSFW0JUAMeDeCmO21laD9S3Y00ur61f8l4enxdsZAWHpZBA5uxVqrJYFGW2FGTJN7fHADVYCc5I1TYe85NNz5fBpQZDZD";
                    //var _instagram_business_account = itemUser.Obj;//"id da pagina
                    //var _userId = itemUser.userId;//"id do usuario no metrics ;

                    if (!access_token_suspensos.Exists(e => e.AccessToken == _access_token))
                    {
                        //Lendo todos os Stories
                        var result = Library.GetDataGraphAsync<Model.Stories>(_access_token,
                                listTokensUser.Where(w => w.access_token == _access_token).FirstOrDefault().Obj +
                                    "/stories?fields=media_url,id,permalink,media_type,caption,username,timestamp",
                                    "", listTokensUser.Where(w => w.access_token == _access_token).Select(s => s.userId).ToList()).Result;

                        //Detalhe de cada Stories
                        if (result != null && (result.data.Count > 0))
                        {
                            WebClient webClient = new WebClient();
                            foreach (var _item in listTokensUser.Where(w => w.access_token == _access_token).ToList())
                            {
                                var head = Library.SetDataGraphAsync<Model.Stories>(_access_token, "setDataStories", _item.userId, _item.userId, result).Result;
                            }
                            var oque = "";
                            foreach (var it in result.data)
                            {
                                try
                                {
                                    if (it.media_type == "IMAGE")
                                    {
                                        var remoteFileUrl = it.media_url;
                                        var _urlLocal = @"c:\output\" + it.id + ".jpg";
                                        oque = "628" + _urlLocal;
                                        if (!File.Exists(_urlLocal))
                                        {
                                            webClient.DownloadFile(remoteFileUrl, _urlLocal);
                                            oque = "632" + remoteFileUrl;
                                            var resultName = UploadFile.UploadFileLoad(_urlLocal, it.id + ".jpg");
                                        }
                                    }

                                    if (it.media_type == "VIDEO")
                                    {
                                        var remoteFileUrl = it.media_url;
                                        var _urlLocal = @"c:\output\" + it.id + ".MP4";
                                        oque = "641" + _urlLocal + "(" + remoteFileUrl +")";
                                        if (!File.Exists(_urlLocal) && !string.IsNullOrEmpty(remoteFileUrl))
                                        {
                                            webClient.DownloadFile(remoteFileUrl, _urlLocal);
                                            var resultName = UploadFile.UploadFileLoad(_urlLocal, it.id + ".MP4");
                                            oque = "646" + _urlLocal;
                                            if (!string.IsNullOrEmpty(resultName))
                                            {
                                                oque = "649 delete" + resultName;
                                                System.IO.File.Delete(_urlLocal);
                                            }
                                        }
                                    }

                                    var item = Library.GetDataGraphAsync<Model.StoryInsights>(_access_token, it.id + "/insights?metric=exits,impressions,reach,replies,taps_forward,taps_back", "",
                                        listTokensUser.Where(w => w.access_token == _access_token).Select(s => s.userId).ToList()).Result;
                                    foreach (var _item in listTokensUser.Where(w => w.access_token == _access_token).ToList())
                                    {
                                        oque = "659 + accesstoken:" + _access_token;
                                        var itens = Library.SetDataGraphAsync<Model.StoryInsights>(_access_token, "setDataStoriesInsights", _item.userId, _item.userId, item).Result;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Library.WriteErrorLog(oque + " :: Erro no MakeStories: it.media_url(" + it.media_url + ")" + ex);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task MakePayments()
        {
            string linhadoerro = "";
            try
            {
                linhadoerro = "Lista";
                var ListPayments = await Library.GetListPaymentsPendentAsync<List<string>>();

                if (ListPayments != null)
                {
                    Library.WriteErrorLog("Payments: " + ListPayments.Count());
                    foreach (var item in ListPayments) // Lista de todas as midias
                    {
                        var feito = Library.SetPayment(item).Result;
                    }
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog("MakeMedia," + linhadoerro + "," + ex.Message.ToString());
            }
        }

        private async void MakeInvoices()
        {
            string linhadoerro = "";
            try
            {
                var ListPayments = Library.SetPaymentInvoices();
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog("MakeMedia," + linhadoerro + "," + ex.Message.ToString());
            }
        }

        private async Task MakeCorrecoes()
        {
            DateTime dtInicial = new DateTime(2019, 03, 10);
            for (var D = 2; D < 999; D++)
            {
                dtInicial = dtInicial.AddDays(D);
                var result = await Library.GetCorrecoes<string>(dtInicial);

                if (dtInicial.Date == DateTime.Now.Date)
                    D = 999;
            }
        }

        private async Task<bool> MakeInfluencersInc()
        {
            var linhaerro = 0;
            setSetting();
            try
            {
                InfluencersMetrics.MongoRep repMongoU = new InfluencersMetrics.MongoRep("ROBO", _settings);
                var lstUsuarioPendentes = await repMongoU.ListarUsuariosPorTipo("5");
                foreach (var it in lstUsuarioPendentes)
                {
                    var _id = it._id.ToString();
                    var usuario = await repMongoU.ListarById<webMetrics.Models.Usuario>(it._id.AsObjectId);
                    if (usuario == null)
                    {
                        break;
                    }

                    var access_token = usuario.FirstOrDefault().Obj.access_token_page;
                    var _instagram_business_account = usuario.FirstOrDefault().Obj.UsuarioInstagram;

                    var _UserId = _id;// HttpContext.Session.GetString("UserId");

                    InfluencersMetrics.MongoRep repMongo = new InfluencersMetrics.MongoRep(_UserId, _settings, _id);
                    var resultUsuario = await GetDataGraphAsync<webMetrics.Models.Graph.Usuario>(access_token, _instagram_business_account +
                        "?fields=biography,id,ig_id,followers_count,follows_count,media_count,name,profile_picture_url,username,website");
                    if (resultUsuario != null)
                    {
                        var resultUsuarioData = await setJsonData(JsonConvert.SerializeObject(resultUsuario), "novaconsulta", _id, "Usuario");

                        var user = usuario.FirstOrDefault().Obj;
                        user.Tipo = "4";
                        user.AgenciaUserId = "5cd38b8c8daa4f1cc82752c2";
                        user.Nome = resultUsuario.name;
                        user.name_page = resultUsuario.name;
                        if (await repMongoU.SalvarAlteracoesUsuario(user, usuario.FirstOrDefault()._id.AsObjectId))
                        {
                            var resultMedia = await GetDataGraphAsync<webMetrics.Models.Graph.Media>(access_token, _instagram_business_account +
                                "/media?fields=caption,children,comments{username,text,id},comments_count,id,ig_id,is_comment_enabled,like_count,media_type,media_url,owner,permalink,shortcode,thumbnail_url,timestamp,username");
                            var resultMediaData = await setJsonData(JsonConvert.SerializeObject(resultMedia), "novaconsulta", _id, "Media");

                            var resultTags = await GetDataGraphAsync<webMetrics.Models.Graph.Tags>(access_token, _instagram_business_account +
                                "/tags?fields=caption,owner,username,media_url,comments_count,like_count&limit=25");
                            var resultTagsData = await setJsonData(JsonConvert.SerializeObject(resultTags), "novaconsulta", _id, "tags");

                            var resultInsightsAge = await GetDataGraphAsync<webMetrics.Models.DTO.Insigth>(access_token, _instagram_business_account +
                                "/insights?metric=audience_gender_age&period=lifetime");
                            var resultInsightsAgeData = await setJsonData(JsonConvert.SerializeObject(resultInsightsAge), "novaconsulta", _id, "InsightsGenderAge");

                            var resultInsightsCity = await GetDataGraphAsync<webMetrics.Models.Graph.InsightsGenderAge>(access_token, _instagram_business_account +
                                "/insights?metric=audience_city&period=lifetime");
                            var resultInsightsCityData = await setJsonData(JsonConvert.SerializeObject(resultInsightsCity), "novaconsulta", _id, "InsightsCity");

                            var resultInsightsUserPro = await GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>(access_token, _instagram_business_account +
                                "/insights?metric=profile_views&period=day");
                            var resultInsightsUserProData = await setJsonData(JsonConvert.SerializeObject(resultInsightsUserPro), "novaconsulta", _id, "UserInsights");

                            var resultInsightsUser = await GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>(access_token, _instagram_business_account +
                                "/insights?metric=impressions,reach&period=week");
                            var resultInsightsUserData = await setJsonData(JsonConvert.SerializeObject(resultInsightsUser), "novaconsulta", _id, "UserInsights");

                            var resultStory = await GetDataGraphAsync<webMetrics.Models.Graph.Stories>(access_token, _instagram_business_account +
                                "/stories?fields=media_url,permalink,username,owner,media_type,shortcode,timestamp");
                            var resultStoryData = await setJsonData(JsonConvert.SerializeObject(resultStory), "novaconsulta", _id, "Stories");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog("MakeInfluencersInc," + linhaerro + "," + ex.Message.ToString());
            }
            return true;
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
                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception)
            {

                return default(T);
            }
        }

        public async Task<string> setJsonData(string json, string namePage, string key, string nameData)
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

                InfluencersMetrics.MongoRep repMongo = new InfluencersMetrics.MongoRep("MetricasInsights", _settings, key);
                if (nameData == "Usuario")
                {
                    var js = JsonConvert.DeserializeObject<webMetrics.Models.Graph.Usuario>(json);
                    await repMongo.GravarOne<webMetrics.Models.Graph.Usuario>(js);
                }
                if (nameData == "Media")
                {
                    var js = JsonConvert.DeserializeObject<webMetrics.Models.Graph.Media>(json);
                    await repMongo.GravarOne<webMetrics.Models.Graph.Media>(js);
                }
                if (nameData == "tags")
                {
                    var js = JsonConvert.DeserializeObject<webMetrics.Models.Graph.Tags>(json);
                    await repMongo.GravarOne<webMetrics.Models.Graph.Tags>(js);
                }
                if (nameData == "Stories")
                {
                    var js = JsonConvert.DeserializeObject<webMetrics.Models.Graph.Stories>(json);
                    await repMongo.GravarOne<webMetrics.Models.Graph.Stories>(js);
                }
                if (nameData == "dataStories")
                {
                    var js = JsonConvert.DeserializeObject<webMetrics.Models.DTO.InsigthStory>(json);
                    await repMongo.GravarOne<webMetrics.Models.DTO.InsigthStory>(js);
                }
                if (nameData == "UserInsights")
                {
                    var js = JsonConvert.DeserializeObject<InfluencersMetricsService.Model.UserInsights>(json);
                    await repMongo.GravarOne<InfluencersMetricsService.Model.UserInsights>(js);
                }
                if (nameData == "Growth")
                {
                    var js = JsonConvert.DeserializeObject<InfluencersMetricsService.Model.UserInsights>(json);
                    await repMongo.GravarOne<InfluencersMetricsService.Model.UserInsights>(js);
                }
                if (nameData == "InsightsCity")
                {
                    var js = JsonConvert.DeserializeObject<webMetrics.Models.Graph.InsightsGenderAge>(json);
                    await repMongo.GravarOne<webMetrics.Models.Graph.InsightsGenderAge>(js);
                }
                if (nameData == "InsightsGenderAge")
                {
                    var js = JsonConvert.DeserializeObject<webMetrics.Models.DTO.Insigth>(json);
                    var lstIns = new List<webMetrics.Models.DTO.InsigthDTO>();
                    js.data.ForEach(f =>
                    {
                        lstIns.Add(new webMetrics.Models.DTO.InsigthDTO()
                        {
                            data = new List<webMetrics.Models.DTO.DatumDTO>()
                                    {
                                    new webMetrics.Models.DTO.DatumDTO()
                                    {
                                        description = f.description,
                                        id = f.id,
                                        name = f.name,
                                        period = f.period,
                                        title = f.title,
                                        values = f.values.Select(s=>new webMetrics.Models.DTO.ValueDTO()
                                        {
                                            end_time = s.end_time,
                                            value = s.value.Select(sv=> new webMetrics.Models.DTO.ValueName()
                                            {
                                                name = sv.Key,
                                                valor = sv.Value.ToString()
                                            }).ToList()
                                        }).ToList()
                                    }
                                    }
                        });
                    });
                    await repMongo.GravarOne<webMetrics.Models.DTO.InsigthDTO>(lstIns.FirstOrDefault());
                }
                return "";
            }
            catch (Exception)
            {
                return "Error#" + nameData;
            }
        }
    }
}
