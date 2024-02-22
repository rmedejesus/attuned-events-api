using MongoDB.Bson;

namespace attuned_events_api.ViewModels
{
    public class EventResource
    {
        public required string EventId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required DateTime Date { get; set; }
        public required string Availability { get; set; }
    }
}
