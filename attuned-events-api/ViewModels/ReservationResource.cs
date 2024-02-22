namespace attuned_events_api.ViewModels
{
    public class ReservationResource
    {
        public required string Message { get; set; }
        public EventResource? Event { get; set; }
        public string? ReservationId { get; set; }
        public int? ReservationAmount { get; set; }
        public string? Cause { get; set; }
    }
}
