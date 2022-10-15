namespace API.Profiles;

using AutoMapper;
using API.Models;
using Data.Models;
using JetBrains.Annotations;

[UsedImplicitly]
public class BaseProfile : Profile
{
    public BaseProfile()
    {
        this.CreateMap<BaseEntity, BaseEntityViewModel>();
    }
}