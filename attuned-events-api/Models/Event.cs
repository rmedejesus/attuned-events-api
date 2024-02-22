using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace attuned_events_api.Models
{
    public class Event
    {
        [BsonId]
        public ObjectId EventId { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("event_date")]
        public required DateTime Date { get; set; }

        [BsonElement("availability")]
        public required int Availability { get; set; }
    }
}
