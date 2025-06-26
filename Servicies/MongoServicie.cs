using Microsoft.Extensions.Options;
using MongoDB.Driver;
using GestionVentas.Models_Mongo;
using GestionVentas.Models;
namespace GestionVentas.Servicies
{
    public class MongoServicie
    {
        private readonly IMongoDatabase _database;

        public MongoServicie(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<FacturaJson> FacturasFirmadas =>
            _database.GetCollection<FacturaJson>("FacturasFirmadas");




    }
}
