using System;

namespace Annstore.Query.Infrastructure
{
    [Serializable]
    public sealed class QueryDbSettings : IQueryDbSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}
