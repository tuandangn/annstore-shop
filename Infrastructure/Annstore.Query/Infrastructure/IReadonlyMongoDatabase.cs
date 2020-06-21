namespace Annstore.Query.Infrastructure
{
    public interface IReadonlyMongoDatabase
    {
        IReadonlyMongoCollection<TEntity> GetCollection<TEntity>(string name);
    }
}
