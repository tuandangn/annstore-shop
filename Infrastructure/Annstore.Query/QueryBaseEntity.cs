using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Annstore.Query
{
    public class QueryBaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int EntityId { get; set; }
    }
}
