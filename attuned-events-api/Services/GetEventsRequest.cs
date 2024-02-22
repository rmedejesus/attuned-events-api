using Amazon.Auth.AccessControlPolicy;
using attuned_events_api.Config;
using attuned_events_api.Models;
using attuned_events_api.ViewModels;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace attuned_events_api.Services
{
    public class GetEventsRequest : IRequest<IEnumerable<EventResource>>
    {

    }

    public class GetEventsRequestHandler : IRequestHandler<GetEventsRequest, IEnumerable<EventResource>>
    {
        private readonly IMongoCollection<Event> _eventCollection;
        private readonly IMongoDatabase db;
        private readonly MongoClient _client;
        private readonly Mapper _autoMapper;

        public GetEventsRequestHandler(IOptions<DBSettings> dbSettings)
        {
            _client = new MongoClient(dbSettings.Value.ConnectionString);
            db = _client.GetDatabase(dbSettings.Value.DatabaseName);
            _eventCollection = db.GetCollection<Event>(MongoCollectionName.Events);
            _autoMapper = AutoMapperConfig.InitializeAutoMapper();
        }

        public async Task<IEnumerable<EventResource>> Handle(GetEventsRequest request, CancellationToken cancellationToken)
        {
            List<EventResource> eventResources = new List<EventResource>();

            List<Event> events = await _eventCollection.Find(Builders<Event>.Filter.Empty).ToListAsync();

            foreach (Event e in events)
            {
                EventResource resource = _autoMapper.Map<EventResource>(e);

                eventResources.Add(resource);
            }

            return eventResources;
        }
    }
}
