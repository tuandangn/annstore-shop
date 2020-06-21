using Annstore.Query.Infrastructure;
using MongoDB.Driver;

namespace Annstore.Query
{
    public sealed class AnnstoreQueryDbContext : IQueryDbContext
    {
        private readonly IReadonlyMongoDatabase _database;

        public AnnstoreQueryDbContext(IQueryDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _database = ReadonlyMongoDatabase.CreateFrom(database);
        }

        public IReadonlyMongoDatabase Database => _database;
    }
}
