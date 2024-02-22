using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace attuned_events_api.Models
{
    public class Reservation
    {
        [BsonId]
        public ObjectId ReservationId { get; set; }

        [BsonElement("event_id")]
        public required ObjectId EventId { get; set; }

        [BsonElement("reservation_amount")]
        public required int ReservationAmount { get; set; }
    }
}
