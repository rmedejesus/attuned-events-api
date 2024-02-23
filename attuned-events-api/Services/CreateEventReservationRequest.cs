using attuned_events_api.Config;
using attuned_events_api.Helpers;
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
        private readonly ReservationResourceHelper _resourceHelper;

        public CreateEventReservationRequestHandler(IOptions<DBSettings> dbSettings)
        {
            _client = new MongoClient(dbSettings.Value.ConnectionString);
            db = _client.GetDatabase(dbSettings.Value.DatabaseName);
            _eventCollection = db.GetCollection<Event>(MongoCollectionName.Events);
            _reservationCollection = db.GetCollection<Reservation>(MongoCollectionName.Reservations);
            _resourceHelper = new ReservationResourceHelper();
        }

        public async Task<ReservationResource> Handle(CreateEventReservationRequest request, CancellationToken cancellationToken)
        {
            Event hostedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            if (request.Parameters.ReservationAmount > hostedEvent.Availability)
            {
                ReservationResource error = _resourceHelper.CreateReservationAmountExceedsResource(hostedEvent, request.Parameters.ReservationAmount);

                return error;
            }
            else if (request.Parameters.ReservationAmount <= 0)
            {
                ReservationResource error = _resourceHelper.CreateReservationAmountIsZeroOrNegativeResource(hostedEvent, request.Parameters.ReservationAmount);

                return error;
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

            ReservationResource createdResource = _resourceHelper.CreateNewReservationResource(reservationId, request.Parameters.ReservationAmount, updatedEvent);

            return createdResource;
        }
    }
}