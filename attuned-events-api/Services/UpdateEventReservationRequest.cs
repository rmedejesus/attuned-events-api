using AutoMapper;
using attuned_events_api.Config;
using attuned_events_api.Models;
using attuned_events_api.RequestParamModels;
using attuned_events_api.ViewModels;
using MediatR;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

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
        private readonly Mapper _autoMapper;

        public UpdateEventReservationRequestHandler(IOptions<DBSettings> dbSettings)
        {
            _client = new MongoClient(dbSettings.Value.ConnectionString);
            db = _client.GetDatabase(dbSettings.Value.DatabaseName);
            _eventCollection = db.GetCollection<Event>(MongoCollectionName.Events);
            _reservationCollection = db.GetCollection<Reservation>(MongoCollectionName.Reservations);
            _autoMapper = AutoMapperConfig.InitializeAutoMapper();
        }

        public async Task<ReservationResource> Handle(UpdateEventReservationRequest request, CancellationToken cancellationToken)
        {
            Event hostedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            if (hostedEvent is null)
            {
                ReservationResource error = new ReservationResource()
                {
                    Message = "Unable to update reservation for the event."
                };

                error.Cause = "Event does not exist.";
                error.ReservationAmount = request.Parameters.ReservationAmount;

                return await Task.Run(() => error);
            }

            EventResource resource = _autoMapper.Map<EventResource>(hostedEvent);

            if (request.Parameters.ReservationAmount <= 0)
            {
                ReservationResource error = new ReservationResource()
                {
                    Message = "Unable to update reservation for the event."
                };

                error.Event = resource;
                error.Cause = "Cannot update reservation having an amount of 0 or less.";
                error.ReservationAmount = request.Parameters.ReservationAmount;

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
                error.ReservationAmount = request.Parameters.ReservationAmount;

                return await Task.Run(() => error);
            }

            Reservation reservation = new Reservation()
            {
                EventId = hostedEvent.EventId,
                ReservationAmount = request.Parameters.ReservationAmount
            };

            int amountToChange = request.Parameters.ReservationAmount - currentReservation.ReservationAmount;

            if (amountToChange > hostedEvent.Availability)
            {
                ReservationResource error = new ReservationResource()
                {
                    Message = "Unable to update reservation for the event."
                };

                error.Event = resource;
                error.Cause = "The amount of reservations exceed the amount of tickets available.";
                error.ReservationAmount = request.Parameters.ReservationAmount;

                return await Task.Run(() => error);
            }

            if (request.Parameters.ReservationAmount == currentReservation.ReservationAmount)
            {
                ReservationResource error = new ReservationResource()
                {
                    Message = "Unable to update reservation for the event."
                };

                error.Cause = "Reservation amount has no change or difference.";
                error.ReservationAmount = request.Parameters.ReservationAmount;

                return await Task.Run(() => error);
            }

            await _eventCollection.UpdateOneAsync(
                Builders<Event>.Filter.Eq(e => e.EventId, request.EventId),
                Builders<Event>.Update.Inc(e => e.Availability, -amountToChange));

            await _reservationCollection.UpdateOneAsync(
                Builders<Reservation>.Filter.Eq(e => e.ReservationId, request.ReservationId),
                Builders<Reservation>.Update.Inc(e => e.ReservationAmount, amountToChange));

            Event updatedEvent = await _eventCollection.Find(Builders<Event>.Filter.Eq(e => e.EventId, request.EventId)).FirstOrDefaultAsync();

            ReservationResource createdResource = new ReservationResource()
            {
                Event = _autoMapper.Map<EventResource>(updatedEvent),
                Message = "Event reservation successfully updated."
            };

            createdResource.ReservationId = request.ReservationId.ToString();
            createdResource.ReservationAmount = request.Parameters.ReservationAmount;

            return createdResource;
        }
    }
}
