using MongoDB.Driver;

namespace Annstore.Query.Infrastructure
{
    public class ReadonlyMongoDatabase : IReadonlyMongoDatabase
    {
        private readonly IMongoDatabase _database;

        private ReadonlyMongoDatabase(IMongoDatabase database)
        {
            _database = database;
        }

        public static ReadonlyMongoDatabase CreateFrom(IMongoDatabase database)
        {
            return new ReadonlyMongoDatabase(database);
        }

        public IReadonlyMongoCollection<TEntity> GetCollection<TEntity>(string name)
        {
            var mongoCollection = _database.GetCollection<TEntity>(name);
            var readonlyCollection = ReadonlyMongoCollection<TEntity>.CreateFrom(mongoCollection);
            return readonlyCollection;
        }
    }
}
