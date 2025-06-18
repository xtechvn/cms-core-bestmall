using Models.MongoDb;
using MongoDB.Driver;
using System.Reflection;
using MongoDB.Bson;
using Utilities;
using Entities.ViewModels.Products;

namespace HuloToys_Service.MongoDb
{
    public class ClientContactMongodbService
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<ClientContactMongoDbModel> bookingCollection;

        public ClientContactMongodbService(IConfiguration configuration) {

            _configuration= configuration;
            //mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=HoanBds
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] +
                ":" + configuration["DataBaseConfig:MongoServer:pwd"] +
                "@" + configuration["DataBaseConfig:MongoServer:Host"] +
                ":" + configuration["DataBaseConfig:MongoServer:Port"] +
                "/?authSource=" + configuration["DataBaseConfig:MongoServer:catalog_core"] + "";

            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(_configuration["DataBaseConfig:MongoServer:catalog_core"]);
            bookingCollection = db.GetCollection<ClientContactMongoDbModel>("ClientContact");
        }
        public async Task<string> Insert(ClientContactMongoDbModel item)
        {
            try
            {
                item.GenID();
                await bookingCollection.InsertOneAsync(item);
                return item._id;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                Utilities.LogHelper.InsertLogTelegram(error_msg);
            }
            return null;

        }
        public async Task<List<ClientContactMongoDbModel>> GetList(string keyword, int page_index = 1, int page_size = 10)
        {
            try
            {
                var keyword_nonunicode = CommonHelper.RemoveUnicode(keyword);
                var filter = Builders<ClientContactMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ClientContactMongoDbModel>.Filter.Or(
                  //Unicode
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.email, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.phone, new BsonRegularExpression($"{keyword}", "i")),
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.message, new BsonRegularExpression($"{keyword}", "i")),
                  //Non-unicode
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.email, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                   Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.phone, new BsonRegularExpression($"{keyword_nonunicode}", "i")),
                  Builders<ClientContactMongoDbModel>.Filter.Regex(x => x.message, new BsonRegularExpression($"{keyword_nonunicode}", "i"))
                );


                var sort_filter = Builders<ClientContactMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.created_date);
                var model = bookingCollection.Find(filterDefinition).Sort(sort_filter_definition);
                model.Options.Skip = page_index < 1 ? 0 : (page_index - 1) * page_size;
                model.Options.Limit = page_size;
                return await model.ToListAsync();

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                Utilities.LogHelper.InsertLogTelegram(error_msg);
            }
            return null;
        }
    }
}
