namespace API.Profiles;

using AutoMapper;
using API.Models;
using Data.Models;

public class BaseProfile : Profile
{
    public BaseProfile()
    {
        this.CreateMap<BaseEntity, BaseEntityViewModel>()
            .ForMember(x => x.Links, conf => conf.MapFrom((_, _, _, context) =>
            {
                return context.Items["links"];
            }));
    }
}