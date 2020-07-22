using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webMetrics.Models;

namespace webMetrics.Repository
{
    public class MongoRepLog
    {
        public IMongoDatabase Database;
        public String DataBaseName = "MetricsLog";
        readonly string conexaoMongoDB = "";
        TimeSpan timeSpan = TimeSpan.FromTicks(DateTime.Now.Ticks);
        string UsuarioInstagram = "";

        private readonly Models.AppSettings _appSettings;
        public MongoRepLog(string Usuario, IOptions<Models.AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            conexaoMongoDB = _appSettings.ConexaoMongoDB;
            var cliente = new MongoClient(conexaoMongoDB);
            Database = cliente.GetDatabase(DataBaseName);
            UsuarioInstagram = Usuario;
        }

        public TimeSpan GetTimeSpan()
        {
            return timeSpan;
        }

        public async Task GravarList<T>(List<T> lst)
        {
            if (lst.Count == 0) return;
            var col = GetOrCreate<Models.ContractClass<List<T>>>(typeof(T).ToString());
            var v = new Models.ContractClass<List<T>>()
            {
                Obj = lst,
                UsuarioInstagram = UsuarioInstagram,
                timeSpan = timeSpan
            };
            await col.InsertOneAsync(v);
        }

        public async Task GravarOne<T>(T lst)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var v = new Models.ContractClass<T>()
            {
                Obj = lst,
                UsuarioInstagram = UsuarioInstagram,
                timeSpan = timeSpan
            };
            await col.InsertOneAsync(v);
        }

        public async Task<List<Models.ContractClass<Models.AutorizacaoMetrica>>> 
            GetMetrica(string usuarioInstagram, string key)
        {
            var col = GetOrCreate<Models.ContractClass<Models.AutorizacaoMetrica>>(typeof(Models.AutorizacaoMetrica).ToString());
            var filter = new BsonDocument("Obj.Key", key);
            var item = await col.FindAsync(filter);
            return item.ToList();
        }

        public async Task<List<Models.ContractClass<T>>> Listar<T>(string usuario)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioInstagram", usuario);
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

        public async Task<Models.ContractClass<T>> Find<T>(string usuario)
        {
            var col = GetOrCreate<Models.ContractClass<T>>(typeof(T).ToString());
            var filter = new BsonDocument("UsuarioInstagram", usuario);
            var item = await col.FindAsync(filter);
            return item.FirstOrDefault();
        }

        public async Task AlterarMetrica(ContractClass<AutorizacaoMetrica> autorizacao)
        {
            var col = GetOrCreate<Models.ContractClass<AutorizacaoMetrica>>(typeof(AutorizacaoMetrica).ToString());
            var updoneresult = await col.UpdateOneAsync(
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Filter.Eq("Obj.Key", autorizacao.Obj.Key),
                   Builders<Models.ContractClass<AutorizacaoMetrica>>.Update.Set("Obj.Status", autorizacao.Obj.Status));
        }

        public async Task<Models.ContractClass<Models.Usuario>> LoginFacebook(string token)
        {
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var filter = new BsonDocument("TokenFacebook", token);
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
            var col = GetOrCreate<Models.ContractClass<Models.Usuario>>(typeof(Models.Usuario).ToString());
            var filter = new BsonDocument("Obj.Email", email);
            var item = await col.FindAsync(filter);
            if (item != null)
            {
                var usuario = item.FirstOrDefault();
                if (usuario.Obj != null)
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
    }
}