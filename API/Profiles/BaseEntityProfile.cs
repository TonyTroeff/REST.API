namespace API.Profiles;

using API.Models;
using AutoMapper;
using Data.Models;

public class BaseEntityProfile : Profile
{
    public BaseEntityProfile()
    {
        this.CreateMap<BaseEntity, BaseEntityViewModel>();
    }
}