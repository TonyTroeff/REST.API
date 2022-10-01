namespace API.Profiles;

using AutoMapper;
using API.Models;
using Data.Models;

public class BaseProfile : Profile
{
    public BaseProfile()
    {
        this.CreateMap<BaseEntity, BaseEntityViewModel>();
    }
}