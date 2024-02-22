using attuned_events_api.RequestParamModels;
using attuned_events_api.Services;
using attuned_events_api.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Net.Http.Headers;
using System.Text;

namespace attuned_events_api.Controllers
{
    [ApiController]
    [Route("/events")]
    [Produces("application/json")]
    public class EventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(IAsyncEnumerable<EventResource>), 200)]
        public async Task<IActionResult> GetEventsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetEventsRequest(), cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [Route("{eventId}/reservations")]
        [ProducesResponseType(typeof(ReservationResource), 200)]
        public async Task<IActionResult> CreateEventReservationAsync(
            [FromRoute] string eventId,
            [FromBody] ReservationModificationParams reservationParams,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new CreateEventReservationRequest() { EventId = ObjectId.Parse(eventId), Parameters = reservationParams }, cancellationToken);

            if (String.IsNullOrEmpty(response.Cause))
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpPatch]
        [Route("{eventId}/reservations/{reservationId}")]
        [ProducesResponseType(typeof(ReservationResource), 200)]
        public async Task<IActionResult> UpdateEventReservationAsync(
            [FromRoute] string eventId,
            [FromRoute] string reservationId,
            [FromBody] ReservationModificationParams reservationParams,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new UpdateEventReservationRequest() { EventId = ObjectId.Parse(eventId), ReservationId = ObjectId.Parse(reservationId), Parameters = reservationParams }, cancellationToken);

            if (String.IsNullOrEmpty(response.Cause))
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpDelete]
        [Route("{eventId}/reservations/{reservationId}")]
        [ProducesResponseType(typeof(ReservationResource), 200)]
        public async Task<IActionResult> DeleteEventReservationAsync(
            [FromRoute] string eventId,
            [FromRoute] string reservationId,
            CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteEventReservationRequest() { EventId = ObjectId.Parse(eventId), ReservationId = ObjectId.Parse(reservationId) }, cancellationToken);

            if (String.IsNullOrEmpty(response.Cause))
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
    }
}
