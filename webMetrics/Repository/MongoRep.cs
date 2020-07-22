using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using webMetrics.Models;

namespace webMetrics.Repository
{
    public class MongoRep
    {
        public IMongoDatabase Database;
        public String DataBaseName = "Metrics";
        readonly string conexaoMongoDB;
        public IMongoDatabase DatabaseCrowler;
        public String DataBaseNameCrowler = "BuscadorLog";
        readonly string conexaoMongoDBCrowler;
        TimeSpan timeSpan = TimeSpan.FromTicks(DateTime.Now.Ticks);
        string UsuarioInstagram = "";
        string usuarioId = "";

        public TimeSpan GetTimeSpan()
        {
            return timeSpan;
        }

        private readonly Models.AppSettings _appSettings;
        public MongoRep(string Usuario, IOptions<Models.AppSettings> appSettings, string userId = "")
        {
            _appSettings = appSettings.Value;
            conexaoMongoDB = _appSettings.ConexaoMongoDB;
            var cliente = new MongoClient(conexaoMongoDB);
            Database = cliente.GetDatabase(DataBaseName);

            conexaoMongoDBCrowler = _appSettings.ConexaoMongoDBCrowler;
            var cliente2 = new MongoClient(conexaoMongoDBCrowler);
            DatabaseCrowler = cliente2.GetDatabase(_appSettings.DataBaseNameCrowler);

            UsuarioInstagram = Usuario;
            usuarioId = userId;
        }

        public async Task GravarList<T>(List<T> lst)
        {
            if (lst.Count == 0) return;
            var col = GetOrCreate<Models.ContractClass<List<T>>>(typeof(T).ToString());
            var v = new Models.ContractClass<List<T>>()
            {
                Obj = lst,
                UsuarioInstagram = UsuarioInstagram,
                UsuarioId = usuarioId,
                timeSpan = timeSpan,
                DateCreation = DatetimeNowHour()
            };
            await col.InsertOneAsync(v);
        }

        public async Task GravarOne<T>(T item)
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
                var v = new Models.ContractClass<T>()
                {
                    Obj = item,
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

        public async Task<List<Models.ContractClass<Models.AutorizacaoMetrica>>>
            GetMetrica(string usuarioInstagram, string key)
        {
            var col = GetOrCreate<Models.ContractClass<Models.AutorizacaoMetrica>>(typeof(Models.AutorizacaoMetrica).ToString());
            var filter = new BsonDocument("Obj.Key", key);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarByOrderId<T>(ObjectId id)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.OrderId", id);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarById<T>(ObjectId id)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("_id", id);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListByInstagramBusinessAccount<T>(string instagrambusinessaccount)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.UsuarioInstagram", instagrambusinessaccount);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarPlano<T>(string code)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.Code", code);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> Listar<T>(string usuario)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioId", usuario);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarByUsuarioInstagram<T>(string usuario)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioInstagram", usuario);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<Models.DTO.CreditoAgencia>>> ListarCreditosPorAgencia(string userId)
        {
            var col = GetOrCreate<Models.ContractClass<Models.DTO.CreditoAgencia>>(typeof(Models.DTO.CreditoAgencia).ToString());
            var filter = new BsonDocument("Obj.UserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<Models.CreditoMetricas>>> ListarCreditos(string userId)
        {
            var col = GetOrCreate<Models.ContractClass<Models.CreditoMetricas>>(typeof(Models.CreditoMetricas).ToString());
            var filter = new BsonDocument("Obj.UserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarUserId<T>(string userId)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.UserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarUsuarioId<T>(string usuarioId)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
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
        public async Task<List<Models.ContractClass<T>>> ListarUserIdByAgencia<T>(string userId)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.AgenciaUserId", userId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarGraphUserId<T>(string userId, DateTime? dtCriacao = null)
        {
            if (dtCriacao == null)
            {
                var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
                var filter = new BsonDocument("UsuarioId", userId);
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
            else
            {
                var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
                var filter = Builders<Models.ContractClass<T>>.Filter
                    .Where(w => w.UsuarioId == userId && w.DateCreation > dtCriacao.Value.AddDays(-1) && w.DateCreation < dtCriacao.Value.AddDays(7)
                );
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
        }

        public async Task<List<Models.ContractClass<T>>> ListarTotalListObjects<T>(DateTime dt)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<Models.ContractClass<T>>.Filter
                .Where(w => w.DateCreation > dt.AddDays(-1) && w.DateCreation < dt
            );
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListGraphByUserIds<T>(List<string> Ids)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<Models.ContractClass<T>>.Filter.Where(w => Ids.Contains(w.UsuarioId));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<string> ConsultaBloqueioProblemasToken(string agenciaId, string nomePage, string instagramBusinessAccount)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter
                .Where(w => w.Obj.AgenciaUserId == agenciaId && w.Obj.UsuarioInstagram == instagramBusinessAccount
                && w.Obj.Tipo == "4" && w.Obj.name_page == nomePage);
            var item = (await col.FindAsync(filter)).ToList();

            if (item.Count() > 0)
            {
                item = item.Where(w => w.Obj.AgenciaUserId != "").ToList();
                var colB = GetOrCreate<Models.ContractClass<InfluencersMetricsService.Model.UserBloqueios>>(typeof(InfluencersMetricsService.Model.UserBloqueios).ToString());
                var filterB = Builders<Models.ContractClass<InfluencersMetricsService.Model.UserBloqueios>>
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

        public async Task<List<Models.ContractClass<T>>> ListarGraphIdByAgencia<T>(string userId)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<Models.ContractClass<T>>.Filter.Where(w => userId == w.UsuarioId);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<Models.Graph.Discovery>>> ListarMediasWithEmotionalByAgencia<T>()
        {
            var col = GetOrCreate<Models.ContractClass<Models.Graph.Discovery>>(typeof(Models.Graph.Discovery).ToString());
            var filter = Builders<Models.ContractClass<Models.Graph.Discovery>>.Filter
                .Eq("Obj.EmotionalProcessed", BsonNull.Value) |
                Builders<Models.ContractClass<Models.Graph.Discovery>>.Filter.Eq("Obj.EmotionalProcessed", false);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarUsuarioByAgencia<T>(List<BsonObjectId> Ids)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<Models.ContractClass<T>>.Filter.Where(w => Ids.Contains(w._id));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarGraphIdByAgencia<T>(List<string> userId)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = Builders<Models.ContractClass<T>>.Filter.Where(w => userId.Contains(w.UsuarioId));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<Models.Usuario>>> ListarUsuariosByUser(string userId)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            ObjectId idUser = new ObjectId();
            ObjectId.TryParse(userId, out idUser);
            if (idUser == ObjectId.Empty)
            {
                var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter.Where(w => w.UsuarioId == userId || w.Obj.UserId == userId);
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
            else
            {
                var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter.Where(w => w.UsuarioId == userId || w.Obj.UserId == userId ||
                    (w._id == idUser && w.Obj.Tipo == "1"));
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
        }

        public async Task<List<Models.ContractClass<Models.Usuario>>> ListarTokensUserByEmail<T>(List<string> emails)
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
                var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter.Where(w => emails.Contains(w.Obj.Email));
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
                var col = GetOrCreate<Models.ContractClass<InfluencersMetricsService.Model.ResumeStoryMidia>>
                    (typeof(InfluencersMetricsService.Model.ResumeStoryMidia).ToString());
                var filter = Builders<Models.ContractClass<InfluencersMetricsService.Model.ResumeStoryMidia>>
                    .Filter.Where(w => w.Obj.id == resume.id && w.Obj.name == resume.name);
                var item = await col.FindAsync(filter);
                var itemList = item.ToList();

                if (itemList.Count() == 0)
                {
                    var v = new Models.ContractClass<InfluencersMetricsService.Model.ResumeStoryMidia>()
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

        public async Task<List<Models.ContractClass<Models.Usuario>>> ListarTokensUserByUserId(List<string> userIds)
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
                var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter.Where(w => userIds.Contains(w.Obj.UserId));
                var item = await col.FindAsync(filter);
                return item.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Models.ContractClass<T>>> ListarInsightsUsers<T>(List<string> userId)
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
                var filter = Builders<Models.ContractClass<T>>.Filter.Where(w => userId.Contains(w.UsuarioId));
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
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter
                .Where(w => w.Obj.AgenciaUserId == agenciaId && w.Obj.UsuarioInstagram == instagramBusinessAccount && w.DateCreation >= DateTime.Now.AddDays(-31)
                && w.Obj.Tipo == "4" && w.Obj.name_page == nomePage);
            var item = await col.FindAsync(filter);
            return (item.Any());
        }

        public async Task<List<Models.ContractClass<Models.Graph.Media>>> GetListMediaInsightsAsync()
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<Models.Graph.Media>>(typeof(Models.Graph.Media).ToString());
                var filter = Builders<Models.ContractClass<Models.Graph.Media>>.Filter
                    .Where(w => w.DateCreation > DateTime.Now.AddDays(-8));
                var item = col.FindAsync(filter).Result;
                var lstItens = item.ToList();
                var lstItensResult = new List<Models.ContractClass<Models.Graph.Media>>();
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
                var col = GetOrCreate<Models.ContractClass<Models.Graph.Discovery>>(typeof(Models.Graph.Discovery).ToString());
                var filter = Builders<Models.ContractClass<Models.Graph.Discovery>>.Filter
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

        public async Task<bool> SetMediaInsight(string userId, string mediaId, int impressions, int reach, int saved, int engagement, string mediaGraphId)
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<Models.Graph.Media>>(typeof(Models.Graph.Media).ToString());
                var _mediaId = new ObjectId(mediaGraphId);

                var filter = Builders<Models.ContractClass<Models.Graph.Media>>.Filter
                    .Where(w => w._id == _mediaId);

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
                var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
                var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter
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
                var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
                var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter
                    .Where(w => w._id == id);
                var item = await col.FindAsync(filter);
                var itemUpdated = item.FirstOrDefault();
                if (itemUpdated != null)
                {
                    var userId = itemUpdated.Obj.UserId;

                    var colPag = GetOrCreate<Models.ContractClass<Models.DTO.PagamentoPage>>(typeof(Models.DTO.PagamentoPage).ToString());
                    var filterPag = Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Filter
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
                var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
                var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter
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

        public async Task<List<Models.ContractClass<Models.DTO.PagamentoPage>>> ListarPendingPay()
        {
            var col = GetOrCreate<Models.ContractClass<Models.DTO.PagamentoPage>>(typeof(Models.DTO.PagamentoPage).ToString());
            var filter = Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Filter
                .Where(w => w.Obj.StatusPagamento == "Pendente");
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<Models.Usuario>>> ListarPendingTokens()
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter
                .Where(w => w.Obj.access_token_page != null && w.DateCreation > DateTime.Now.AddDays(-7)
                && w.Obj.Tipo != "2");
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<Models.Usuario>>> ListarUsersAtivos()
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var filter = Builders<Models.ContractClass<Models.Usuario>>.Filter
                .Where(w => w.Obj.access_token_page != null && w.DateCreation > DateTime.Now.AddDays(-60));
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> ListarEmail<T>(string email)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("Obj.Email", email);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<Models.ContractClass<T>> FindFilter<T>(string _filter, string _value)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument(_filter, _value);
            var item = await col.FindAsync(filter);
            return item.FirstOrDefault();
        }

        public async Task<Models.ContractClass<T>> Find<T>(string usuario)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioInstagram", usuario);
            var item = await col.FindAsync(filter);
            return item.FirstOrDefault();
        }

        public async Task AlterarStatusPagamento(ContractClass<Models.DTO.PagamentoPage> pagamento)
        {
            var col = GetOrCreate<Models.ContractClass<Models.DTO.PagamentoPage>>(typeof(Models.DTO.PagamentoPage).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Filter.Eq("_id", pagamento._id),
                   Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Update.Set("Obj.StatusPagamento", pagamento.Obj.StatusPagamento));

            updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Filter.Eq("_id", pagamento._id),
                   Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Update.Set("Obj.paymentResponse.Status", pagamento.Obj.paymentResponse.Status));

            updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Filter.Eq("_id", pagamento._id),
                   Builders<Models.ContractClass<Models.DTO.PagamentoPage>>.Update.Set("Obj.NextInvoice", pagamento.Obj.NextInvoice));

        }

        public async Task AlterarInvoices(ContractClass<Models.DTO.PagamentoPage> pagamento)
        {
            var col = GetOrCreate<Models.ContractClass<Models.DTO.PagamentoPage>>(typeof(Models.DTO.PagamentoPage).ToString());
            var replaceOneResult = await col.ReplaceOneAsync(
                    doc => doc._id == pagamento._id,
                    pagamento);
        }

        public async Task AlterarMetrica(ContractClass<AutorizacaoMetrica> autorizacao)
        {
            var col = GetOrCreate<Models.ContractClass<AutorizacaoMetrica>>(typeof(AutorizacaoMetrica).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", autorizacao.Obj.Key),
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Update.Set("Obj.Status", autorizacao.Obj.Status));
        }

        public async Task AlterarCredito(ContractClass<Models.CreditoMetricas> credito)
        {
            var col = GetOrCreate<Models.ContractClass<Models.CreditoMetricas>>(typeof(Models.CreditoMetricas).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.CreditoMetricas>>.Filter.Eq("_id", credito._id),
                   Builders<Models.ContractClass<Models.CreditoMetricas>>.Update.Set("Obj.Debito", credito.Obj.Debito));
        }

        public async Task AlterarUsuarioToken(ContractClass<Models.Usuario> usuario)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<Models.ContractClass<Models.Usuario>>.Update.Set(s => s.Obj.access_token_page, usuario.Obj.access_token_page));
        }

        public async Task AlterarUsuarioCredito(ContractClass<Models.Usuario> usuario)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<Models.ContractClass<Models.Usuario>>.Update.Set(s => s.Obj.StatusCredito, usuario.Obj.StatusCredito));

            updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<Models.ContractClass<Models.Usuario>>.Update.Set(s => s.Obj.DataUsoCredito, usuario.Obj.DataUsoCredito));

        }

        public async Task<bool> AlterarProcessamento(ObjectId id)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Graph.Discovery>>(typeof(Models.Graph.Discovery).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Graph.Discovery>>.Filter.Eq("_id", id),
                   Builders<Models.ContractClass<Models.Graph.Discovery>>.Update.Set("Obj.reprocessado", true));

            return true;
        }

        public async Task<bool> AlterarProcessamentoUsuario(ObjectId id)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Graph.Usuario>>(typeof(Models.Graph.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Graph.Usuario>>.Filter.Eq("_id", id),
                   Builders<Models.ContractClass<Models.Graph.Usuario>>.Update.Set("Obj.reprocessado", true));

            return true;
        }

        public async Task<bool> ConfirmarProcessamentoUsuario(ObjectId id, DateTime dtCreation)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Graph.Usuario>>(typeof(Models.Graph.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Graph.Usuario>>.Filter.Eq("_id", id),
                   Builders<Models.ContractClass<Models.Graph.Usuario>>.Update.Set("Obj.DateCreation", dtCreation));

            return true;
        }

        public async Task<bool> ExcluirVinculo(ObjectId id)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Usuario>>.Filter.Eq("_id", id),
                   Builders<Models.ContractClass<Models.Usuario>>.Update.Set("Obj.AgenciaUserId", ""));

            return true;
        }
        public async Task<bool> ExcluirVinculoDiscovery(ObjectId id)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Graph.Discovery>>(typeof(Models.Graph.Discovery).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Graph.Discovery>>.Filter.Eq("_id", id),
                   Builders<Models.ContractClass<Models.Graph.Discovery>>.Update.Set("UsuarioId", ""));

            return true;
        }

        public async Task AlterarToken(ContractClass<Models.Usuario> usuario)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<Models.ContractClass<Models.Usuario>>.Update.Set("Obj.access_token_page", usuario.Obj.access_token_page));
            updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<Models.ContractClass<Models.Usuario>>.Update.Set("Obj.name_page", usuario.Obj.name_page));

            updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<Models.Usuario>>.Filter.Eq("_id", usuario._id),
                   Builders<Models.ContractClass<Models.Usuario>>.Update.Set("Obj.UsuarioInstagram", usuario.Obj.UsuarioInstagram));

        }

        public async Task<Models.ContractClass<Models.Usuario>> LoginFacebook(string token)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
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

        public async Task<Models.ContractClass<Models.Usuario>> Login(string email, string senha)
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
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


        public async Task<Models.ContractClass<Models.Usuario>> LoginId(ObjectId id)
        {
            try
            {
                var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
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

        private IMongoCollection<T> GetOrCreate<T>(string nameCollection, bool connectToCrowlerDb = false)
        {
            var col = (connectToCrowlerDb == true ? DatabaseCrowler : Database).GetCollection<T>(nameCollection);
            if (col == null)
            {
                Database.CreateCollection(nameCollection);
                col = Database.GetCollection<T>(nameCollection);
            }
            return col;
        }

        public async Task AtualizarAutorizacaoMetrica(string key, AutorizacaoMetrica usuarioAutorizacaoMetrica)
        {
            var col = GetOrCreate<Models.ContractClass<AutorizacaoMetrica>>(typeof(AutorizacaoMetrica).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", key),
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Update
                   .Set("Obj.PowerFull", usuarioAutorizacaoMetrica.PowerFull));
            updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", key),
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Update
                   .Set("Obj.Seguidores", usuarioAutorizacaoMetrica.Seguidores));

            updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", key),
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Update
                   .Set("Obj.Aprovado", usuarioAutorizacaoMetrica.Aprovado));
        }
        public async Task<List<UsuarioCredito>> ListarAgencias()
        {
            var colUsuario = GetOrCreate<webMetrics.Models.ContractClass<Usuario>>
                (typeof(Usuario).ToString());
            var colCreditos = GetOrCreate<webMetrics.Models.ContractClass<CreditoMetricas>>
                (typeof(CreditoMetricas).ToString());

            var query =
            from e in colUsuario.AsQueryable<webMetrics.Models.ContractClass<Usuario>>()
            where e.Obj.Tipo == "2"
            select new UsuarioCredito
            {
                _id = e._id,
                NomeAgencia = e.Obj.NomeAgencia,
                Telefone = e.Obj.Telefone,
                Nome = e.Obj.Nome,
                DataCriacao = e.Obj.DataCriacao,
                Email = e.Obj.Email
            };
            var _lst = query.ToList().Select(s => new { _id = s._id.Value.ToString() }).ToList();

            var queryCreditos =
            from c in colCreditos.AsQueryable<webMetrics.Models.ContractClass<CreditoMetricas>>()
            where c.Obj != null && (c.Obj.DataValidade >= DateTime.Now)
            select c;

            List<UsuarioCredito> lst = new List<UsuarioCredito>(); //  && (c.Obj.Qtd > c.Obj.Debito.Value)
            query.ToList().ForEach(f =>
            {
                var cred = queryCreditos.ToList().Where(w => w.Obj.UserId == f._id.ToString())
                .Select(s => s.Obj).ToList();
                lst.Add(new UsuarioCredito()
                {
                    _id = f._id,
                    Creditos = cred
                    ,
                    DataCriacao = f.DataCriacao,
                    Debito = cred.Sum(s => s.Debito),
                    Qtd = cred.Sum(s => s.Qtd),
                    Email = f.Email,
                    Nome = f.Nome,
                    NomeAgencia = f.NomeAgencia,
                    Telefone = f.Telefone
                });
            });

            return lst;// query.ToList();
        }

        public async Task<List<Models.ContractClass<Models.Graph.Media>>> GetMediaWithoutInsight(int dias = 60)
        {
            //var col = GetOrCreate<Models.ContractClass<Models.Graph.Media>>(typeof(Models.Graph.Media).ToString());
            try
            {
                var col = GetOrCreate<Models.ContractClass<Models.Graph.Media>>(typeof(Models.Graph.Media).ToString());

                var query = Builders<Models.ContractClass<Models.Graph.Media>>.Filter.And(
                    Builders<Models.ContractClass<Models.Graph.Media>>.Filter.Ne(_ => _.Obj, null),
                    Builders<Models.ContractClass<Models.Graph.Media>>.Filter.Gte(_ => _.DateCreation, new DateTime(2019, 11, 01)),
                    Builders<Models.ContractClass<Models.Graph.Media>>.Filter.Ne(_ => _.Obj.data, null),
                    Builders<Models.ContractClass<Models.Graph.Media>>.Filter.ElemMatch(_ => _.Obj.data, _ => _.Insight == false)
                );

                var item = await col.FindAsync(query);
                return item.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected SortDefinition<T> SorterBuilderPagination<T>(string fieldSortName, bool asc)
        {
            return asc ? Builders<T>.Sort.Ascending(fieldSortName) : Builders<T>.Sort.Descending(fieldSortName);
        }

        public async Task<SearchEntity> SearchInfluencer(
            string search,
            bool onlyInstagram,
            bool onlyYoutube,
            bool onlyTwitter,
            bool onlyTiktopk,
            bool onlyLinkedin,
            bool onlyPodCasts,
            int currentPage,
            int displayLenght,
            int maxAge,
            int minAge,
            bool genderMale,
            bool genderFemale,
            int MinFollowers,
            int MaxFollowers)
        {
            var col = GetOrCreate<InfluencerEntity>("SearchInfluencers.Domain.Entities.InfluencerEntity", connectToCrowlerDb: true);
            var builders = Builders<InfluencerEntity>.Filter;
            //var filter = builders.Where(w => w.Search.Contains(search)
            //|| w.Search.Contains(search.ToUpper().Trim())
            //|| w.Name.Contains(search)
            //|| w.UserName.Contains(search));

            var filter = builders.Regex(u => u.Search, new BsonRegularExpression("/" + search + "/i"));

            if (MinFollowers > 0 && MaxFollowers == 0)
            {
                filter = filter
                    & builders.Gte(nameof(InfluencerEntity.Followers), MinFollowers);
            }
            else if (MaxFollowers > 0 && MinFollowers < MaxFollowers)
            {
                filter = filter
                    & builders.Gte(nameof(InfluencerEntity.Followers), MinFollowers)
                    & builders.Lte(nameof(InfluencerEntity.Followers), MaxFollowers);
            }
            if (minAge > 0 || maxAge < 100)
            {
                filter = filter
                    & builders.Gte(nameof(InfluencerEntity.Age), minAge)
                    & builders.Lte(nameof(InfluencerEntity.Age), maxAge);
            }
            if (genderMale && genderFemale)
            {
                //filter = filter & (builders.Eq(nameof(InfluencerEntity.Gender), "Female") | builders.Eq(nameof(InfluencerEntity.Gender), "Male"));
            }
            else if (genderMale)
            {
                filter = filter & builders.Eq(nameof(InfluencerEntity.Gender), "Male");
            }
            else if (genderFemale)
            {
                filter = filter & builders.Eq(nameof(InfluencerEntity.Gender), "Female");
            }
            if (onlyInstagram || onlyYoutube || onlyTwitter || onlyTiktopk || onlyLinkedin || onlyPodCasts)
            {
                var socialNetworks = new List<string>();
                if (onlyInstagram)
                {
                    socialNetworks.Add("I");
                }
                if (onlyYoutube)
                {
                    socialNetworks.Add("Y");
                }
                if (onlyTwitter)
                {
                    socialNetworks.Add("T");
                }
                if (onlyTiktopk)
                {
                    socialNetworks.Add("TK");
                }
                if (onlyLinkedin)
                {
                    socialNetworks.Add("L");
                }
                if (onlyPodCasts)
                {
                    socialNetworks.Add("P");
                }
                filter = filter & builders.In(a => a.Origin, socialNetworks);
            }

            var builtSort = this.SorterBuilderPagination<InfluencerEntity>(nameof(InfluencerEntity.Followers), false);
            var skip = ((currentPage - 1) * displayLenght);
            var countAll = await col.CountDocumentsAsync(filter);
            var result = countAll == 0 ? new List<InfluencerEntity>() :
                await col.Aggregate(new AggregateOptions { AllowDiskUse = true })
                .Match(filter)
                .Sort(builtSort)
                .Skip(skip)
                .Limit(displayLenght).ToListAsync();
            var categories = result.Where(a => a.Categories != null).SelectMany(a => a.Categories.Select(b => b.ToUpper().Trim())).Distinct().ToList();
            if (categories == null || categories.Count == 0)
            {
                categories = new List<string>()
                {
                    "NENHUMA CATEGORIA"
                };
            }
            return new SearchEntity()
            {
                WordSearch = search,
                CurrentPage = currentPage,
                CountAllResults = countAll,
                DisplayLenght = displayLenght,
                PaginationButtonsNameValue = null,
                Influencers = result,
                Categories = categories,
                FilterByInstagram = onlyInstagram,
                FilterByYoutube = onlyYoutube,
                FilterByTwitter = onlyTwitter,
                FilterByTiktopk = onlyTiktopk,
                FilterByLinkedin = onlyLinkedin,
                FilterByPodCasts = onlyPodCasts
            };
        }
        public async Task<InfluencerDetailInfo> GetInfluencerDetail(string id)
        {
            var col = GetOrCreate<InfluencerEntity>("SearchInfluencers.Domain.Entities.InfluencerEntity", connectToCrowlerDb: true);
            var idObj = new BsonObjectId(new ObjectId(id));
            var filter = Builders<InfluencerEntity>.Filter.Eq(a => a._id, idObj);
            var info = await col.Find(filter).FirstOrDefaultAsync();
            if (info == null)
            {
                return null;
            }
            var culture = new CultureInfo("pt-BR");
            List<string> images = new List<string>();
            List<string> categories = new List<string>();
            InfluencerPhoto influencerDetail = null;
            if (info != null)
            {
                if (info.Origin == "I")
                {
                    influencerDetail = GetOrCreate<InfluencerPhoto>("Infs.Model.Influencers", true).Find(a => a.UserName == info.UserName).FirstOrDefault();
                    if (influencerDetail != null && influencerDetail.Photos != null)
                    {
                        images = influencerDetail.Photos.Select(a => a.Url).ToList();
                        categories = influencerDetail.Photos.Select(a => a.CategoryDescription).Distinct().ToList();
                    }
                }
                else if (info.Origin == "Y")
                {
                    //https://i1.ytimg.com/vi/BoegMDZIIiU/0.jpg
                    //https://www.youtube.com/watch?v=BoegMDZIIiU
                    var yt = GetOrCreate<InfluencerPhotoYoutube>("Infs.Model.YoutubeInfluencers", true).Find(a => a.Channel == info.UserName).FirstOrDefault();
                    if (yt != null && yt.Videos != null)
                    {
                        images = yt.Videos.Select(a =>
                        {
                            var videoId = a.Url.Split(new string[] { "watch?v=" }, StringSplitOptions.None)[1];
                            var result = "https://i1.ytimg.com/vi/" + videoId + "/0.jpg";
                            return result;
                        }).ToList();
                        categories = yt.Videos.Select(a => a.CategoryDescription).Distinct().ToList();
                    }
                }
                else if (info.Origin == "T")
                {
                    //https://i1.ytimg.com/vi/BoegMDZIIiU/0.jpg
                    //https://www.youtube.com/watch?v=BoegMDZIIiU
                    var yt = GetOrCreate<InfluencerTwitter>("Infs.Model.TwitterInfluencers", true).Find(a => a.UserName == info.UserName).FirstOrDefault();
                    if (yt != null && yt.Tweets != null)
                    {
                        images = yt.Tweets.Where(b => string.IsNullOrWhiteSpace(b.URLPhoto) == false).Select(a =>
                           {
                               return a.URLPhoto;
                           }).ToList();
                        categories = yt.Tweets.Select(a => a.CategoryDescription).Distinct().ToList();
                    }
                }
            }
            //categories.AddRange(info.Categories.Select(b => b.ToUpper().Trim()).Distinct().ToList());
            info.Categories = categories.Distinct().ToList();
            if (info.Categories == null || info.Categories.Count == 0)
            {
                info.Categories = new List<string>()
                {
                    "NENHUMA CATEGORIA"
                };
            }
            return new InfluencerDetailInfo(userName: info.UserName,
                description: info.Text,
                postsCount: "0",
                followers: info.Followers.ToString("N0", culture),
                following: influencerDetail == null ? "0" : influencerDetail.Followings,
                engagement: "0",
                impressions: "0",
                reach: "0",
                reachTags: categories,
                posts: images.Count > 0 ? images.Select(a => new PostInfluencer()
                {
                    Comments = "-",
                    Likes = "-",
                    UrlImg = a
                }).ToList() :
                Enumerable.Range(1, 9).Select(a => new PostInfluencer()
                {
                    Comments = "-",
                    Likes = "-",
                    UrlImg = "/images/noavailable.jpg"
                }).ToList(),
                contacts: info.Name,
                imageProfile: info.ImageProfile,
                origin: info.Origin);
        }

        public async Task<CrowlerInfoRealTime> GetInfoHomeCrowler()
        {
            var col = GetOrCreate<InfluencerEntity>("SearchInfluencers.Domain.Entities.InfluencerEntity", true);
            var minForInfluencer = 30000;
            var result = await col.Aggregate()
                .Group(a => "", g => new
                {
                    influencers = g.LongCount(b => b.Followers >= minForInfluencer),
                    studyInsta = g.LongCount(b => b.Followers < minForInfluencer && b.Origin == "I"),
                    influecencersInsta = g.LongCount(b => b.Followers >= minForInfluencer && b.Origin == "I"),
                    studyTwitter = g.LongCount(b => b.Followers < minForInfluencer && b.Origin == "T"),
                    influecencersTwitter = g.LongCount(b => b.Followers >= minForInfluencer && b.Origin == "T"),
                    studyYoutube = g.LongCount(b => b.Followers < minForInfluencer && b.Origin == "Y"),
                    influecencersYoutube = g.LongCount(b => b.Followers >= minForInfluencer && b.Origin == "Y")
                })
                .ToListAsync();
            var infoCrowler = result?.FirstOrDefault() ?? new
            {
                influencers = (long)0,
                studyInsta = (long)0,
                influecencersInsta = (long)0,
                studyTwitter = (long)0,
                influecencersTwitter = (long)0,
                studyYoutube = (long)0,
                influecencersYoutube = (long)0
            };
            //var f1 = Builders<InfluencerEntity>.Filter.Gte(a => a.Followers, 30000);
            //var BiggerThan30 = await col.CountDocumentsAsync(f1);
            //var f2 = Builders<InfluencerEntity>.Filter.Eq(a => a.Origin, "I");
            //var InstaApproved = await col.CountDocumentsAsync(f2);
            //var f3 = Builders<InfluencerEntity>.Filter.Eq(a => a.Origin, "T");
            //var TwitteApproved = await col.CountDocumentsAsync(f3);
            //var f4 = Builders<InfluencerEntity>.Filter.Eq(a => a.Origin, "Y");
            //var YoutubeApproved = await col.CountDocumentsAsync(f4);
            var culture = new CultureInfo("pt-BR");
            return new CrowlerInfoRealTime()
            {
                BiggerThan30 = infoCrowler.influencers.ToString("N0", culture),
                InstaApproved = infoCrowler.influecencersInsta.ToString("N0", culture),
                InstaInStudy = infoCrowler.studyInsta.ToString("N0", culture),
                TwitteApproved = infoCrowler.influecencersTwitter.ToString("N0", culture),
                TwitteInStudy = infoCrowler.studyTwitter.ToString("N0", culture),
                YoutubeApproved = infoCrowler.influecencersYoutube.ToString("N0", culture),
                YoutubeInStudy = infoCrowler.studyYoutube.ToString("N0", culture)
            };
        }
    }
}