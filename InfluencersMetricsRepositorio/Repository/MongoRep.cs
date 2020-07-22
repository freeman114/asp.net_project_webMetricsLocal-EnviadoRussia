using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
//using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
//using MongoDB.Driver.Core.Bindings;
//using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using webMetrics;
using webMetrics.Models;

namespace InfluencersMetrics
{
    public class MongoRep
    {
        public IMongoDatabase Database;
        public String DataBaseName = "Metrics";
        readonly string conexaoMongoDB;
        TimeSpan timeSpan = TimeSpan.FromTicks(DateTime.Now.Ticks);
        string UsuarioInstagram = "";
        string usuarioId = "";

        public TimeSpan GetTimeSpan()
        {
            return timeSpan;
        }

        private readonly webMetrics.Models.AppSettings _appSettings;
        public MongoRep(string Usuario, IOptions<webMetrics.Models.AppSettings> appSettings, string userId = "")
        {
#if DEBUG
            //DataBaseName = "HmlMetrics";
#endif
            _appSettings = appSettings.Value;
            conexaoMongoDB = _appSettings.ConexaoMongoDB;

            var cliente = new MongoClient(conexaoMongoDB);

            Database = cliente.GetDatabase(DataBaseName);

            UsuarioInstagram = Usuario;
            usuarioId = userId;
        }

        public async Task GravarList<T>(List<T> lst)
        {
            if (lst.Count == 0) return;
            var col = GetOrCreate<webMetrics.Models.ContractClass<List<T>>>(typeof(T).ToString());
            var v = new webMetrics.Models.ContractClass<List<T>>()
            {
                Obj = lst,
                UsuarioInstagram = UsuarioInstagram,
                UsuarioId = usuarioId,
                timeSpan = timeSpan,
                DateCreation = DatetimeNowHour()
            };
            await col.InsertOneAsync(v);
        }

        public async Task GravarStoryBestHour<T>(T lst, string UsuarioId)
        {
            usuarioId = UsuarioId;
            await GravarOne<T>(lst);
        }

        public async Task GravarOne<T>(T lst)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
                var v = new webMetrics.Models.ContractClass<T>()
                {
                    Obj = lst,
                    UsuarioInstagram = UsuarioInstagram,
                    UsuarioId = usuarioId,
                    timeSpan = timeSpan,
                    DateCreation = DatetimeNowHour()
                };
                await col.InsertOneAsync(v);
            }
            catch (Exception)
            {

            }
        }

        public static DateTime DatetimeNowHour()
        {
            return new DateTime(DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                DateTime.Now.Hour,
                0,
                0);
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.AutorizacaoMetrica>>>
            GetMetrica(string usuarioInstagram, string key)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.AutorizacaoMetrica>>(typeof(webMetrics.Models.AutorizacaoMetrica).ToString());
            var filter = new BsonDocument("Obj.Key", key);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarById<T>(ObjectId id)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("_id", id);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListByInstagramBusinessAccount<T>(string instagrambusinessaccount)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.UsuarioInstagram", instagrambusinessaccount);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarPlano<T>(string code)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.Code", code);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> Listar<T>(string usuario)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioId", usuario);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarByUsuarioInstagram<T>(string usuario)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioInstagram", usuario);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.DTO.CreditoAgencia>>> ListarCreditosPorAgencia(string userId)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.DTO.CreditoAgencia>>(typeof(webMetrics.Models.DTO.CreditoAgencia).ToString());
            var filter = new BsonDocument("Obj.UserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.CreditoMetricas>>> ListarCreditos(string userId)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.CreditoMetricas>>(typeof(webMetrics.Models.CreditoMetricas).ToString());
            var filter = new BsonDocument("Obj.UserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarUserId<T>(string userId)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.UserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarUsuarioId<T>(string usuarioId)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.UsuarioId", usuarioId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        /// <summary>
        /// Lista os usuarios da agencia
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarUserIdByAgencia<T>(string userId)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.AgenciaUserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.StoryInsights>>> ListarGraphStoryId()
        {
            /*var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter.In("Obj.data.2._id", storyIdFull) &
                Builders<webMetrics.Models.ContractClass<T>>.Filter.Where(w => w.DateCreation > dtCriacao.AddDays(-1) && w.DateCreation < dtCriacao
            );

            var item = await col.FindAsync(filter);
            */


            var col = GetOrCreate<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.StoryInsights>>
                (typeof(InfluencersMetricsService.Model.StoryInsights).ToString());

            var query =
            from e in col.AsQueryable<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.StoryInsights>>()
            where e.DateCreation >= DateTime.Now.AddDays(-7) && e.DateCreation <= DateTime.Now.AddDays(-1)
            //&& e.Obj!= null && ( (e.Obj.data !=null) && (e.Obj.data.Exists(x=>storyIdFull.Contains(x.id))))
            select e;

            return query.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarGraphUserId<T>(string userId, DateTime? dtCriacao = null)
        {
            if (dtCriacao == null)
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
                var filter = new BsonDocument("UsuarioId", userId);
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
            else
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter
                    .Where(w => w.UsuarioId == userId && w.DateCreation > dtCriacao.Value.AddDays(-1) && w.DateCreation < dtCriacao.Value.AddDays(7)
                );
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarTotalListObjects<T>(DateTime dt)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter
                .Where(w => w.DateCreation > dt.AddDays(-1) && w.DateCreation < dt
            );
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListGraphByUserIds<T>(List<string> Ids)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter.Where(w => Ids.Contains(w.UsuarioId));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<string> ConsultaBloqueioProblemasToken(string agenciaId, string nomePage, string instagramBusinessAccount)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter
                .Where(w => w.Obj.AgenciaUserId == agenciaId && w.Obj.UsuarioInstagram == instagramBusinessAccount && w.Obj.Tipo == "4" && w.Obj.name_page == nomePage);
            var item = (await col.FindAsync(filter)).ToList();

            if (item.Count() > 0)
            {
                item = item.Where(w => w.UsuarioId != "").ToList();
                var colB = GetOrCreate<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.UserBloqueios>>(typeof(InfluencersMetricsService.Model.UserBloqueios).ToString());
                var filterB = Builders<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.UserBloqueios>>
                        .Filter.Where(w => item.FirstOrDefault().UsuarioId == w.UsuarioId);
                var itemBloqueio = (await colB.FindAsync(filterB)).ToList();

                if (itemBloqueio.Count() > 0)
                {
                    return itemBloqueio.FirstOrDefault()._id.ToString();
                }
                else
                {
                    var id = "";
                    return id;
                }
            }
            return "";
        }

        public async Task LimparBestHour()
        {
            var col = GetOrDrop<webMetrics.Models.ContractClass<List<InfluencersMetricsService.Model.StoryBestHour>>>
                (typeof(List<InfluencersMetricsService.Model.StoryBestHour>).ToString());
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarGraphIdByAgencia<T>(string userId)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter.Where(w => userId == w.UsuarioId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>> ListarMediasWithEmotionalByAgencia<T>()
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>(typeof(webMetrics.Models.Graph.Discovery).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>.Filter
                .Eq("Obj.EmotionalProcessed", BsonNull.Value) |
                Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>.Filter.Eq("Obj.EmotionalProcessed", false)
                ;
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarUsuarioByAgencia<T>(List<BsonObjectId> Ids)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter.Where(w => Ids.Contains(w._id));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarGraphIdByAgencia<T>(List<string> userId)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter.Where(w => userId.Contains(w.UsuarioId));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>> ListarTokensUserByEmail<T>(List<string> emails)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Where(w => emails.Contains(w.Obj.Email));
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SetStoryInsightResume(InfluencersMetricsService.Model.ResumeStoryMidia resume)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.ResumeStoryMidia>>
                    (typeof(InfluencersMetricsService.Model.ResumeStoryMidia).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.ResumeStoryMidia>>
                    .Filter.Where(w => w.Obj.id == resume.id && w.Obj.name == resume.name);
                var item = await col.FindAsync(filter);
                var itemList = item.ToList();

                if (itemList.Count() == 0)
                {
                    var v = new webMetrics.Models.ContractClass<InfluencersMetricsService.Model.ResumeStoryMidia>()
                    {
                        Obj = resume,
                        UsuarioInstagram = UsuarioInstagram,
                        UsuarioId = usuarioId,
                        timeSpan = timeSpan,
                        DateCreation = DatetimeNowHour()
                    };
                    await col.InsertOneAsync(v);
                    return true;
                }
                else//if (itemList.FirstOrDefault().DateCreation <= DatetimeNowHour())
                {
                    var itemUpdated = itemList.FirstOrDefault();
                    itemUpdated.Obj.name = resume.name;
                    itemUpdated.Obj.description = resume.description;
                    itemUpdated.Obj.period = resume.period;
                    itemUpdated.Obj.title = resume.title;
                    itemUpdated.Obj.values = resume.values;

                    var replaceOneResult = await col.ReplaceOneAsync(
                        doc => doc._id == itemUpdated._id,
                        itemUpdated);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>> ListarTokensUserByUserId(List<string> userIds)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Where(w => userIds.Contains(w.Obj.UserId));
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarInsightsUsers<T>(List<string> userId)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<T>>.Filter.Where(w => userId.Contains(w.UsuarioId));
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ListUserIdByAgencia(string agenciaId, string instagramBusinessAccount, string nomePage)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter
                .Where(w => w.Obj.AgenciaUserId == agenciaId && w.Obj.UsuarioInstagram == instagramBusinessAccount
                && w.Obj.Tipo == "4" && w.Obj.name_page == nomePage);
            var item = await col.FindAsync(filter);
            return (item.Any());
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>> GetListMediaInsightsAsync()
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>(typeof(webMetrics.Models.Graph.Media).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>.Filter
                    .Where(w => w.DateCreation > DateTime.Now.AddDays(-8));
                var item = col.FindAsync(filter).Result;
                var lstItens = item.ToList();
                var lstItensResult = new List<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>();
                foreach (var it in lstItens)
                {
                    if (it.Obj != null &&
                        (it.Obj.data != null &&
                        (it.Obj.data.Count > 0)))
                    {
                        lstItensResult.Add(it);
                    }
                }

                var lstMedia = lstItensResult.Where(w => w.Obj.data.Where(d => d.Insight == false).Count() > 0);
                return (lstMedia.ToList());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SetMediaDoscoveryEmotional(string _id)
        {
            try
            {
                var id = new ObjectId(_id);
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>(typeof(webMetrics.Models.Graph.Discovery).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>.Filter
                    .Where(w => w.DateCreation > DateTime.Now.AddDays(-8) && w._id == id);

                var item = await col.FindAsync(filter);
                var itemUpdated = item.FirstOrDefault();
                itemUpdated.Obj.EmotionalProcessed = true;

                var replaceOneResult = await col.ReplaceOneAsync(
                    doc => doc._id == itemUpdated._id,
                    itemUpdated);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SetMediaInsight(string userId, string mediaId, int impressions, int reach, int saved, int engagement)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>(typeof(webMetrics.Models.Graph.Media).ToString());
                //var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>.Filter
                //    .Where(w => w.DateCreation > DateTime.Now.AddDays(-8) && w.UsuarioId == userId);

                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>.Filter
                    .Where(w => w.UsuarioId == userId);

                var item = col.Find(filter);
                var itemUpdated = item.FirstOrDefault();

                itemUpdated.Obj.data.Where(w => w.id == mediaId).FirstOrDefault().Insight = true;
                itemUpdated.Obj.data.Where(w => w.id == mediaId).FirstOrDefault().Impressions = impressions;
                itemUpdated.Obj.data.Where(w => w.id == mediaId).FirstOrDefault().Reach = reach;
                itemUpdated.Obj.data.Where(w => w.id == mediaId).FirstOrDefault().Saved = saved;
                itemUpdated.Obj.data.Where(w => w.id == mediaId).FirstOrDefault().Engagement = engagement;

                var replaceOneResult = await col.ReplaceOneAsync(
                    doc => doc._id == itemUpdated._id,
                    itemUpdated);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> changePassword(ObjectId id, string oldPass, string newPass)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter
                    .Where(w => w._id == id);

                var item = await col.FindAsync(filter);
                var itemUpdated = item.FirstOrDefault();
                if (itemUpdated != null)
                {
                    if (itemUpdated.Obj.Senha == oldPass)
                    {
                        itemUpdated.Obj.Senha = newPass;
                        var replaceOneResult = await col.ReplaceOneAsync(doc => doc._id == itemUpdated._id, itemUpdated);
                        return "";
                    }
                    else
                    {
                        return "#senhainvalida";
                    }
                }
                else
                {
                    return "#erro#Usuário não encontrado.";
                }
            }
            catch (Exception ex)
            {
                return "#erro#" + ex.Message;
            }
        }

        public async Task<string> CancelarAssinatura(ObjectId id)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter
                    .Where(w => w._id == id);
                var item = await col.FindAsync(filter);
                var itemUpdated = item.FirstOrDefault();
                if (itemUpdated != null)
                {
                    var userId = itemUpdated.Obj.UserId;

                    var colPag = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>(typeof(webMetrics.Models.DTO.PagamentoPage).ToString());
                    var filterPag = Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Filter
                        .Where(w => w.Obj.Usuario.UserId == userId);
                    var itemPag = await colPag.FindAsync(filterPag);
                    var itemPagUpdated = itemPag.FirstOrDefault();

                    if (itemPagUpdated != null)
                    {
                        itemPagUpdated.Obj.StatusPagamento = "SolicitarCancelamento";
                        var replaceOneResult = await colPag.ReplaceOneAsync(doc => doc._id == itemPagUpdated._id, itemPagUpdated);
                        return "";
                    }
                    else
                    {
                        return "#assinatura não encontrada";
                    }
                }
                else
                {
                    return "#erro#Usuário não encontrado.";
                }
            }
            catch (Exception ex)
            {
                return "#erro#" + ex.Message;
            }
        }

        public async Task<bool> resetSenha(string email)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
                var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter
                    .Where(w => w.Obj.Email == email);

                var item = await col.FindAsync(filter);
                var itemUpdated = item.FirstOrDefault();
                itemUpdated.Obj.Senha = "12influencers3";

                var replaceOneResult = await col.ReplaceOneAsync(doc => doc._id == itemUpdated._id, itemUpdated);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>> ListarPendingPay()
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>(typeof(webMetrics.Models.DTO.PagamentoPage).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Filter
                .Where(w => w.Obj.StatusPagamento == "Pendente");
            var item = await col.FindAsync(filter);
            return item.ToList();
        }
        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>> ListarPendingPayInvoice()
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>(typeof(webMetrics.Models.DTO.PagamentoPage).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Filter
                .Where(w => w.Obj.StatusPagamento == "Assinatura Ativa" && w.Obj.NextInvoice <= DateTime.Now);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<List<InfluencersMetricsService.Model.StoryBestHour>>>> ListarStoriesBest()
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<List<InfluencersMetricsService.Model.StoryBestHour>>>
                (typeof(List<InfluencersMetricsService.Model.StoryBestHour>).ToString());
            var filterB = Builders<webMetrics.Models.ContractClass<List<InfluencersMetricsService.Model.StoryBestHour>>>.Filter
                .Where(w => w.timeSpan >= TimeSpan.FromHours(-8));

            var item = await col.FindAsync(filterB);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.Stories>>> ListarStoriesActual(List<string> lstUsuarioIds)
        {

            //var lst = await GetList<InfluencersMetricsService.Model.Stories>(DateTime.Now.AddDays(-7), "InfluencersMetricsService.Model.Stories");
            ////var col = GetOrCreate<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.Stories>>(typeof(InfluencersMetricsService.Model.Stories).ToString());
            ////var filter = Builders<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.Stories>>.Filter
            ////    .Where(w => lstUsuarioIds.Contains(w.UsuarioId) && w.DateCreation < DateTime.Now.Date && w.DateCreation > DateTime.Now.Date.AddDays(-7));

            //var item = lst.Where(w => lstUsuarioIds.Contains(w.UsuarioId));
            //;
            ////var item = await col.FindAsync(filter);
            //return item.ToList();

            return null;
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>> ListarPendingTokens()
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter
                .Where(w => w.Obj.access_token_page != null && w.DateCreation > DateTime.Now.AddDays(-60)
                && w.Obj.Tipo != "2");
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>> ListarUsersAtivos()
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var filter = Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter
                .Where(w => w.Obj.access_token_page != null && w.DateCreation > DateTime.Now.AddDays(-60));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<webMetrics.Models.ContractClass<T>>> ListarEmail<T>(string email)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.Email", email);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<webMetrics.Models.ContractClass<T>> FindFilter<T>(string _filter, string _value)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument(_filter, _value);
            var item = await col.FindAsync(filter);
            return item.FirstOrDefault();
        }

        public async Task<webMetrics.Models.ContractClass<T>> Find<T>(string usuario)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioInstagram", usuario);
            var item = await col.FindAsync(filter);
            return item.FirstOrDefault();
        }

        public async Task AlterarStatusPagamento(ContractClass<webMetrics.Models.DTO.PagamentoPage> pagamento)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>(typeof(webMetrics.Models.DTO.PagamentoPage).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Filter.Eq("_id", pagamento._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Update.Set("Obj.StatusPagamento", pagamento.Obj.StatusPagamento));

            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Filter.Eq("_id", pagamento._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Update.Set("Obj.paymentResponse.Status", pagamento.Obj.paymentResponse.Status));

        }

        public async Task AlterarNextInvoice(ContractClass<webMetrics.Models.DTO.PagamentoPage> pagamento)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>(typeof(webMetrics.Models.DTO.PagamentoPage).ToString());
            var del = await col.DeleteOneAsync(Builders<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>.Filter.Eq("_id", pagamento._id));
            await col.InsertOneAsync(pagamento);
        }

        public async Task AlterarInvoices(ContractClass<webMetrics.Models.DTO.PagamentoPage> pagamento)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.DTO.PagamentoPage>>(typeof(webMetrics.Models.DTO.PagamentoPage).ToString());
            var replaceOneResult = await col.ReplaceOneAsync(
                    doc => doc._id == pagamento._id,
                    pagamento);
        }

        public async Task AlterarMetrica(ContractClass<AutorizacaoMetrica> autorizacao)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<AutorizacaoMetrica>>(typeof(AutorizacaoMetrica).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", autorizacao.Obj.Key),
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Update.Set("Obj.Status", autorizacao.Obj.Status));
        }

        public async Task AlterarCredito(ContractClass<webMetrics.Models.CreditoMetricas> credito)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.CreditoMetricas>>(typeof(webMetrics.Models.CreditoMetricas).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.CreditoMetricas>>.Filter.Eq("_id", credito._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.CreditoMetricas>>.Update.Set("Obj.Debito", credito.Obj.Debito));
        }

        public async Task AlterarUsuarioToken(ContractClass<webMetrics.Models.Usuario> usuario)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update.Set(s => s.Obj.access_token_page, usuario.Obj.access_token_page));
        }

        public async Task AlterarUsuarioCredito(ContractClass<webMetrics.Models.Usuario> usuario)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update.Set(s => s.Obj.StatusCredito, usuario.Obj.StatusCredito));

            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update.Set(s => s.Obj.DataUsoCredito, usuario.Obj.DataUsoCredito));

        }

        public async Task<bool> AlterarProcessamento(ObjectId id)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>(typeof(webMetrics.Models.Graph.Discovery).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>.Filter.Eq("_id", id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>.Update.Set("Obj.reprocessado", true));

            return true;
        }

        public async Task<bool> AlterarProcessamentoUsuario(ObjectId id)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Usuario>>(typeof(webMetrics.Models.Graph.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Usuario>>.Filter.Eq("_id", id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Usuario>>.Update.Set("Obj.reprocessado", true));

            return true;
        }

        public async Task<bool> ExcluirVinculo(ObjectId id)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq("_id", id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update.Set("Obj.AgenciaUserId", ""));

            return true;
        }
        public async Task<bool> ExcluirVinculoDiscovery(ObjectId id)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>(typeof(webMetrics.Models.Graph.Discovery).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>.Filter.Eq("_id", id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Discovery>>.Update.Set("UsuarioId", ""));

            return true;
        }

        public async Task AlterarToken(ContractClass<webMetrics.Models.Usuario> usuario)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update.Set("Obj.access_token_page", usuario.Obj.access_token_page));
            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update.Set("Obj.name_page", usuario.Obj.name_page));

            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update.Set("Obj.UsuarioInstagram", usuario.Obj.UsuarioInstagram));

        }

        public async Task<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>> LoginFacebook(string token)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var filter = new BsonDocument("Obj.UserId", token);
            var item = await col.FindAsync(filter);
            if (item != null)
            {
                return item.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>> Login(string email, string senha)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
                var filter = new BsonDocument("Obj.Email", email);
                var item = await col.FindAsync(filter);
                if (item != null)
                {
                    var usuario = item.FirstOrDefault();
                    if (usuario != null && (usuario.Obj != null))
                    {
                        if (usuario.Obj.Senha == senha)
                        {
                            return usuario;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }


        public async Task<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>> LoginId(ObjectId id)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
                var filter = new BsonDocument("_id", id);
                var item = await col.FindAsync(filter);
                if (item != null)
                {
                    var usuario = item.FirstOrDefault();
                    return usuario;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private IMongoCollection<T> GetOrCreate<T>(string nameCollection)
        {
            var col = Database.GetCollection<T>(nameCollection);
            if (col == null)
            {
                Database.CreateCollection(nameCollection);
                col = Database.GetCollection<T>(nameCollection);
            }
            return col;
        }
        private IMongoCollection<T> GetOrDrop<T>(string nameCollection)
        {
            var col = Database.GetCollection<T>(nameCollection);
            if (col == null)
            {
                Database.DropCollection(nameCollection);
                col = Database.GetCollection<T>(nameCollection);
            }
            return col;
        }

        public async Task AtualizarAutorizacaoMetrica(string key, AutorizacaoMetrica usuarioAutorizacaoMetrica)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<AutorizacaoMetrica>>(typeof(AutorizacaoMetrica).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", key),
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Update
                   .Set("Obj.PowerFull", usuarioAutorizacaoMetrica.PowerFull));
            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", key),
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Update
                   .Set("Obj.Seguidores", usuarioAutorizacaoMetrica.Seguidores));

            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", key),
                   Builders<webMetrics.Models.ContractClass<AutorizacaoMetrica>>.Update
                   .Set("Obj.Aprovado", usuarioAutorizacaoMetrica.Aprovado));
        }

        public async Task<bool> SalvarAlteracoesUsuario(webMetrics.Models.Usuario user, ObjectId id)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq(e => e._id, id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update
                   .Set(s => s.Obj.AgenciaUserId, user.AgenciaUserId));
            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq(e => e._id, id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update
                   .Set(s => s.Obj.Tipo, user.Tipo));
            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq(e => e._id, id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update
                   .Set(s => s.Obj.Nome, user.Nome));
            updoneresult = await col.UpdateOneAsync(
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Filter.Eq(e => e._id, id),
                   Builders<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>.Update
                   .Set(s => s.Obj.name_page, user.name_page));
            return true;

        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>> GetMediaWithoutInsight(int dias = 60)
        {
            //var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>(typeof(webMetrics.Models.Graph.Media).ToString());
            try
            {
                var strCommand = "{ eval: \"getInsightMediaPending('" + DateTime.Now.AddDays(-dias).ToString("yyyy-MM-dd hh:mm") + "')\" }";
                var command = new JsonCommand<BsonDocument>(strCommand);
                var result = Database.RunCommand(command)["retval"];
                if (result != null)
                {
                    return BsonSerializer.Deserialize<List<webMetrics.Models.ContractClass<webMetrics.Models.Graph.Media>>>(result.ToJson());
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.Stories>>> GetStoriesPending(List<string> lstUsuarioIds)
        {
            try
            {
                var col = GetOrCreate<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.Stories>>(typeof(InfluencersMetricsService.Model.Stories).ToString());

                var query =
                from e in col.AsQueryable<webMetrics.Models.ContractClass<InfluencersMetricsService.Model.Stories>>()
                where e.DateCreation >= DateTime.Now.AddDays(-7) && lstUsuarioIds.Contains(e.UsuarioId)
                select e;

                return query.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>>
            ListarUsuariosPorTipo(string tipo)
        {
            var col = GetOrCreate<webMetrics.Models.ContractClass<webMetrics.Models.Usuario>>(typeof(webMetrics.Models.Usuario).ToString());
            var filter = new BsonDocument("Obj.Tipo", tipo);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }
    }
}

public static class MongoClientExtensions
{
    /// <summary>
    /// Evaluates the specified javascript within a MongoDb database
    /// </summary>
    /// <param name="database">MongoDb Database to execute the javascript</param>
    /// <param name="javascript">Javascript to execute</param>
    /// <returns>A BsonValue result</returns>
    public static async Task<BsonValue> EvalAsync(this IMongoDatabase database, string javascript)
    {
        var client = database.Client as MongoClient;

        if (client == null)
            throw new ArgumentException("Client is not a MongoClient");

        var function = new BsonJavaScript(javascript);
        var op = new EvalOperation(database.DatabaseNamespace, function, null);

        using (var writeBinding = new WritableServerBinding(client.Cluster, new CoreSessionHandle(NoCoreSession.Instance)))
        {
            return await op.ExecuteAsync(writeBinding, CancellationToken.None);
        }
    }
}