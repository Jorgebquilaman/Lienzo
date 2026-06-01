using AutoMapper;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;

namespace Lienzo.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Building, BuildingDto>();
        CreateMap<Building, BuildingDetailDto>();

        CreateMap<Classroom, ClassroomDto>()
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building.Name))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<Classroom, ClassroomDetailDto>()
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building.Name))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<Classroom, ClassroomSummaryDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<Reservation, ReservationDto>()
            .ForMember(d => d.ClassroomName, o => o.MapFrom(s => s.Classroom.Name))
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Classroom.Building != null ? s.Classroom.Building.Name : null))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Date, o => o.MapFrom(s => s.Date.ToDateTime(TimeOnly.MinValue)))
            .ForMember(d => d.ActividadId, o => o.MapFrom(s => s.ActividadId));

        CreateMap<Announcement, AnnouncementDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.TargetAudience, o => o.MapFrom(s => s.TargetAudience.ToString()));

        CreateMap<Announcement, AnnouncementListItemDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<Periodo, PeriodoDto>()
            .ForMember(d => d.FechaInicio, o => o.MapFrom(s => s.FechaInicio.ToString("yyyy-MM-dd")))
            .ForMember(d => d.FechaFin, o => o.MapFrom(s => s.FechaFin.ToString("yyyy-MM-dd")))
            .ForMember(d => d.TipoPeriodoNombre, o => o.MapFrom(s => s.TipoPeriodo != null ? s.TipoPeriodo.Nombre : null));

        CreateMap<ClassroomType, string>().ConvertUsing(e => e.ToString());
    }
}
