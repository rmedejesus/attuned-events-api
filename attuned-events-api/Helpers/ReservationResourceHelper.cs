using attuned_events_api.Config;
using attuned_events_api.Models;
using attuned_events_api.ViewModels;
using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace attuned_events_api.Helpers
{
    public class ReservationResourceHelper
    {
        private readonly Mapper _autoMapper;

        public ReservationResourceHelper()
        {
            _autoMapper = AutoMapperConfig.InitializeAutoMapper();
        }

        public ReservationResource CreateNewReservationResource(ObjectId reservationId, int reservationAmount, Event updatedEvent)
        {
            ReservationResource createdResource = new ReservationResource()
            {
                Event = _autoMapper.Map<EventResource>(updatedEvent),
                Message = "Event reservation successfully created."
            };

            createdResource.ReservationId = reservationId.ToString();
            createdResource.ReservationAmount = reservationAmount;

            return createdResource;
        }

        public ReservationResource CreateReservationUpdatedResource(ObjectId reservationId, int reservationAmount, Event updatedEvent)
        {
            ReservationResource updatedResource = new ReservationResource()
            {
                Event = _autoMapper.Map<EventResource>(updatedEvent),
                Message = "Event reservation successfully updated."
            };

            updatedResource.ReservationId = reservationId.ToString();
            updatedResource.ReservationAmount = reservationAmount;

            return updatedResource;
        }

        public ReservationResource CreateReservationDeletedResource(ObjectId reservationId, Event updatedEvent)
        {
            ReservationResource createdResource = new ReservationResource()
            {
                Event = _autoMapper.Map<EventResource>(updatedEvent),
                Message = "Event reservation successfully deleted."
            };

            createdResource.ReservationId = reservationId.ToString();

            return createdResource;
        }

        public ReservationResource CreateEventDoesNotExistResource()
        {
            ReservationResource error = new ReservationResource()
            {
                Message = "Unable to update reservation for the event."
            };

            error.Cause = "Event does not exist.";

            return error;
        }

        public ReservationResource CreateReservationDoesNotExistResource()
        {
            ReservationResource error = new ReservationResource()
            {
                Message = "Unable to update reservation for the event."
            };

            error.Cause = "Reservation does not exist.";

            return error;
        }

        public ReservationResource CreateReservationAmountIsZeroOrNegativeResource(Event hostedEvent, int reservationAmount)
        {
            EventResource resource = _autoMapper.Map<EventResource>(hostedEvent);

            ReservationResource error = new ReservationResource()
            {
                Message = "Unable to update reservation for the event."
            };

            error.Event = resource;
            error.Cause = "Cannot update reservation having an amount of 0 or less.";
            error.ReservationAmount = reservationAmount;

            return error;
        }

        public ReservationResource CreateReservationAmountExceedsResource(Event hostedEvent, int reservationAmount)
        {
            EventResource resource = _autoMapper.Map<EventResource>(hostedEvent);

            ReservationResource error = new ReservationResource()
            {
                Message = "Unable to update reservation for the event."
            };

            error.Event = resource;
            error.Cause = "The amount of reservations exceed the amount of tickets available.";
            error.ReservationAmount = reservationAmount;

            return error;
        }

        public ReservationResource CreateReservationAmountNoChangeResource(int reservationAmount)
        {
            ReservationResource error = new ReservationResource()
            {
                Message = "Unable to update reservation for the event."
            };

            error.Cause = "Reservation amount has no change or difference.";
            error.ReservationAmount = reservationAmount;

            return error;
        }
    }
}