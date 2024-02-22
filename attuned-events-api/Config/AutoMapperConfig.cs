using attuned_events_api.Models;
using attuned_events_api.ViewModels;
using AutoMapper;

namespace attuned_events_api.Config
{
    public class AutoMapperConfig
    {
        public static Mapper InitializeAutoMapper()
        {
            //Provide all the Mapping Configuration
            var config = new MapperConfiguration(cfg =>
            {
                //Configuring Employee and EmployeeDTO
                cfg.CreateMap<Event, EventResource>()
                    .ForMember(dest => dest.Availability,
                        opt => opt.MapFrom(src => src.Availability == 1 ? Convert.ToString(src.Availability) + " ticket available" : Convert.ToString(src.Availability) + " tickets available"));
            });

            //Create an Instance of Mapper and return that Instance
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}
