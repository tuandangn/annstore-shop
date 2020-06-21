using Annstore.Query.Infrastructure;

namespace Annstore.Query
{
    public interface IQueryDbContext
    {
        IReadonlyMongoDatabase Database { get; }
    }
}
