namespace Annstore.Query.Infrastructure
{
    public sealed class QueryDbSettings : IQueryDbSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}
