using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Annstore.Query
{
    [Serializable]
    public class QueryBaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int EntityId { get; set; }
    }
}
