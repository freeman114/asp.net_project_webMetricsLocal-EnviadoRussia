using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webMetrics.Repository
{
    public class MongoSearchRep
    {
        public IMongoDatabase Database;
        public String DataBaseName = "BuscadorLog";
        readonly string conexaoMongoDB;

        public MongoSearchRep()
        {
            conexaoMongoDB = "mongodb://admin:wigroup@168.235.111.153:27017";
#if DEBUG
            //conexaoMongoDB = "mongodb://localhost:27017";
#endif
            var cliente = new MongoClient(conexaoMongoDB);
            Database = cliente.GetDatabase(DataBaseName);
        }

        public async Task<BsonDocument> GetValue(string _command)
        {
            var result = await Database.RunCommandAsync<BsonDocument>(new BsonDocument("count", _command));
            return result;
        }

        public async Task<string> ExecuteMongoDBCommand(String mongoCmd)
        {
            try
            {
                //var strCommand = "{ eval: \"getInsightMediaPending('" + DateTime.Now.AddDays(-dias).ToString("yyyy-MM-dd hh:mm") + "')\" }";
                var strCommand = "{ count: \"db.getCollection('Infs.Model.Influencers').find({'Type':1})\" }";
                var command = new JsonCommand<BsonDocument>(strCommand);
                var result = Database.RunCommand(command);//["retval"];
                if (result != null)
                {
                    return result.ToJson();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<long> GetCountEqual(string collectionName, string fieldName, dynamic value)
        {
            var col = Database.GetCollection<BsonDocument>(collectionName);
            var filter = new BsonDocument(fieldName, new BsonDocument("$eq", value));
            var result = await col.CountDocumentsAsync(filter);
            return result;
        }
        public async Task<long> GetCountNotEqual(string collectionName, string fieldName, dynamic value)
        {
            var col = Database.GetCollection<BsonDocument>(collectionName);
            var filter = new BsonDocument(fieldName, new BsonDocument("$ne", value));
            var result = await col.CountDocumentsAsync(filter);
            return result;
        }

        private async Task<IMongoCollection<T>> GetOrCreate<T>(string nameCollection)
        {
            //var nameCollection = typeof(T).Name;
            var col = Database.GetCollection<T>(nameCollection);
            if (col == null)
            {
                await Database.CreateCollectionAsync(nameCollection);
                col = Database.GetCollection<T>(nameCollection);
            }
            return col;
        }
    }
}
