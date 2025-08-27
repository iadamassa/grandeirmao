using AutoMapper;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SubjectToResearch, SubjectToResearchDto>().ReverseMap();
        CreateMap<SubjectExample, SubjectExampleDto>().ReverseMap();
        CreateMap<SiteCategory, SiteCategoryDto>().ReverseMap();
        CreateMap<Site, SiteDto>().ReverseMap();
        CreateMap<SiteLink, SiteLinkDto>().ReverseMap();
        CreateMap<Anomaly, AnomalyDto>()
            .ForMember(dest => dest.SiteLinkName, opt => opt.MapFrom(src => src.SiteLink != null ? src.SiteLink.Name : ""))
            .ForMember(dest => dest.SiteLinkUrl, opt => opt.MapFrom(src => src.SiteLink != null ? src.SiteLink.Url : ""))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.SubjectToResearch != null ? src.SubjectToResearch.Name : ""))
            .ReverseMap();
    }
}