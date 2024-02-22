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
    public class CreateEventReservationRequest : IRequest<ReservationResource>
    {
        public ObjectId EventId { get; set; }

        public required ReservationModificationParams Parameters { get; set; }
    }

    public class CreateEventReservationRequestHandler : IRequestHandler<CreateEventReservationRequest, ReservationResource>
    {
        private readonly IMongoCollection<Event> _eventCollection;
        private readonly IMongoCollection<Reservation> _reservationCollection;
        private readonly IMongoDatabase db;
        private readonly MongoClient _client;
        private readonly Mapper _autoMapper;

        public CreateEventReservationRequestHandler(IOptions<DBSettings> dbSettings)
        {
            _client = new MongoClient(dbSettings.Value.ConnectionString);
            db = _client.GetDatabase(dbSettings.Value.DatabaseName);
            _eventCollection = db.GetCollection<Event>(MongoCollectionName.Events);
            _reservationCollection = db.GetCollection<Reservation>(MongoCollectionName.Reservations);
            _autoMapper = AutoMapperConfig.InitializeAutoMapper();
        }

        public async Task<ReservationResource> Handle(CreateEventReservationRequest request, CancellationToken cancellationToken)
        {
            Event hostedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            EventResource resource = _autoMapper.Map<EventResource>(hostedEvent);

            if (request.Parameters.ReservationAmount > hostedEvent.Availability)
            {
                ReservationResource error = new ReservationResource()
                { 
                    Event = resource,
                    Message = "Unable to create reservation for the event."
                };

                error.Cause = "The amount of reservations exceed the amount of tickets available.";
                error.ReservationAmount = request.Parameters.ReservationAmount;

                return await Task.Run(() => error);
            }
            else if (request.Parameters.ReservationAmount <= 0)
            {
                ReservationResource error = new ReservationResource()
                {
                    Event = resource,
                    Message = "Unable to create reservation for the event."
                };

                error.Cause = "Cannot create reservation having an amount of 0 or less.";
                error.ReservationAmount = request.Parameters.ReservationAmount;

                return await Task.Run(() => error);
            }

            ObjectId reservationId = ObjectId.GenerateNewId();

            Reservation reservation = new Reservation()
            { 
                ReservationId = reservationId,
                EventId = hostedEvent.EventId,
                ReservationAmount = request.Parameters.ReservationAmount
            };

            await _eventCollection.UpdateOneAsync(
                Builders<Event>.Filter.Eq(e => e.EventId, request.EventId),
                Builders<Event>.Update.Inc(e => e.Availability, -request.Parameters.ReservationAmount));

            await _reservationCollection.InsertOneAsync(reservation);

            Event updatedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();
            Reservation createdReservation = await _reservationCollection.Find(Builders<Reservation>.Filter.Eq(e => e.ReservationId, request.EventId)).FirstOrDefaultAsync();

            ReservationResource createdResource = new ReservationResource()
            {
                Event = _autoMapper.Map<EventResource>(updatedEvent),
                Message = "Event reservation successfully created."
            };

            createdResource.ReservationId = reservationId.ToString();
            createdResource.ReservationAmount = request.Parameters.ReservationAmount;

            return createdResource;
        }
    }
}
