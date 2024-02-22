using attuned_events_api.Config;
using attuned_events_api.Models;
using attuned_events_api.RequestParamModels;
using attuned_events_api.ViewModels;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace attuned_events_api.Services
{
    public class DeleteEventReservationRequest : IRequest<ReservationResource>
    {
        public ObjectId EventId { get; set; }
        public ObjectId ReservationId { get; set; }
    }

    public class DeleteEventReservationRequestHandler : IRequestHandler<DeleteEventReservationRequest, ReservationResource>
    {
        private readonly IMongoCollection<Event> _eventCollection;
        private readonly IMongoCollection<Reservation> _reservationCollection;
        private readonly IMongoDatabase db;
        private readonly MongoClient _client;
        private readonly Mapper _autoMapper;

        public DeleteEventReservationRequestHandler(IOptions<DBSettings> dbSettings)
        {
            _client = new MongoClient(dbSettings.Value.ConnectionString);
            db = _client.GetDatabase(dbSettings.Value.DatabaseName);
            _eventCollection = db.GetCollection<Event>(MongoCollectionName.Events);
            _reservationCollection = db.GetCollection<Reservation>(MongoCollectionName.Reservations);
            _autoMapper = AutoMapperConfig.InitializeAutoMapper();
        }

        public async Task<ReservationResource> Handle(DeleteEventReservationRequest request, CancellationToken cancellationToken)
        {
            Event hostedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            if (hostedEvent is null)
            {
                ReservationResource error = new ReservationResource()
                {
                    Message = "Unable to update reservation for the event."
                };

                error.Cause = "Event does not exist.";

                return await Task.Run(() => error);
            }

            Reservation currentReservation = await _reservationCollection.Find(Builders<Reservation>.Filter.Eq(e => e.ReservationId, request.ReservationId)).FirstOrDefaultAsync();

            if (currentReservation is null)
            {
                ReservationResource error = new ReservationResource()
                {
                    Message = "Unable to update reservation for the event."
                };

                error.Cause = "Reservation does not exist.";

                return await Task.Run(() => error);
            }

            await _eventCollection.UpdateOneAsync(
                Builders<Event>.Filter.Eq(e => e.EventId, request.EventId),
                Builders<Event>.Update.Inc(e => e.Availability, currentReservation.ReservationAmount));

            await _reservationCollection.DeleteOneAsync(Builders<Reservation>.Filter.Eq(e => e.ReservationId, request.ReservationId));

            Event updatedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            ReservationResource createdResource = new ReservationResource()
            {
                Event = _autoMapper.Map<EventResource>(updatedEvent),
                Message = "Event reservation successfully deleted."
            };

            createdResource.ReservationId = request.ReservationId.ToString();

            return createdResource;
        }
    }
}
