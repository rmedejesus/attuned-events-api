using AutoMapper;
using attuned_events_api.Config;
using attuned_events_api.Models;
using attuned_events_api.RequestParamModels;
using attuned_events_api.ViewModels;
using MediatR;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using attuned_events_api.Helpers;

namespace attuned_events_api.Services
{
    public class UpdateEventReservationRequest : IRequest<ReservationResource>
    {
        public ObjectId EventId { get; set; }
        public ObjectId ReservationId { get; set; }
        public required ReservationModificationParams Parameters { get; set; }
    }

    public class UpdateEventReservationRequestHandler : IRequestHandler<UpdateEventReservationRequest, ReservationResource>
    {
        private readonly IMongoCollection<Event> _eventCollection;
        private readonly IMongoCollection<Reservation> _reservationCollection;
        private readonly IMongoDatabase db;
        private readonly MongoClient _client;
        private readonly ReservationResourceHelper _resourceHelper;

        public UpdateEventReservationRequestHandler(IOptions<DBSettings> dbSettings)
        {
            _client = new MongoClient(dbSettings.Value.ConnectionString);
            db = _client.GetDatabase(dbSettings.Value.DatabaseName);
            _eventCollection = db.GetCollection<Event>(MongoCollectionName.Events);
            _reservationCollection = db.GetCollection<Reservation>(MongoCollectionName.Reservations);
            _resourceHelper = new ReservationResourceHelper();
        }

        public async Task<ReservationResource> Handle(UpdateEventReservationRequest request, CancellationToken cancellationToken)
        {
            Event hostedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            if (hostedEvent is null)
            {
                ReservationResource error = _resourceHelper.CreateEventDoesNotExistResource();
                return error;
            }

            if (request.Parameters.ReservationAmount <= 0)
            {
                ReservationResource error = _resourceHelper.CreateReservationAmountIsZeroOrNegativeResource(hostedEvent, request.Parameters.ReservationAmount);

                return error;
            }

            Reservation currentReservation = await _reservationCollection.Find(Builders<Reservation>.Filter.Eq(e => e.ReservationId, request.ReservationId)).FirstOrDefaultAsync();

            if (currentReservation is null)
            {
                ReservationResource error = _resourceHelper.CreateReservationDoesNotExistResource();

                return error;
            }

            Reservation reservation = new Reservation()
            {
                EventId = hostedEvent.EventId,
                ReservationAmount = request.Parameters.ReservationAmount
            };

            int amountToChange = request.Parameters.ReservationAmount - currentReservation.ReservationAmount;

            if (amountToChange > hostedEvent.Availability)
            {
                ReservationResource error = _resourceHelper.CreateReservationAmountExceedsResource(hostedEvent, request.Parameters.ReservationAmount);

                return error;
            }

            if (request.Parameters.ReservationAmount == currentReservation.ReservationAmount)
            {
                ReservationResource error = _resourceHelper.CreateReservationAmountNoChangeResource(request.Parameters.ReservationAmount);

                return error;
            }

            await _eventCollection.UpdateOneAsync(
                Builders<Event>.Filter.Eq(e => e.EventId, request.EventId),
                Builders<Event>.Update.Inc(e => e.Availability, -amountToChange));

            await _reservationCollection.UpdateOneAsync(
                Builders<Reservation>.Filter.Eq(e => e.ReservationId, request.ReservationId),
                Builders<Reservation>.Update.Inc(e => e.ReservationAmount, amountToChange));

            Event updatedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            ReservationResource updatedResource = _resourceHelper.CreateReservationUpdatedResource(request.ReservationId, request.Parameters.ReservationAmount, updatedEvent);

            return updatedResource;
        }
    }
}