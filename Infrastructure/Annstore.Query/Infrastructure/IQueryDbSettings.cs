namespace Annstore.Query.Infrastructure
{
    public interface IQueryDbSettings
    {
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }
    }
}
