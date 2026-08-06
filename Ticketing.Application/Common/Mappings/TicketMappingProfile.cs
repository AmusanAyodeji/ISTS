using AutoMapper;
using Ticketing.Application.DTOs;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Common.Mappings;

public class TicketMappingProfile : Profile
{
    public TicketMappingProfile()
    {
        CreateMap<CreateTicketDto, Ticket>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedToId, opt => opt.Ignore())
            .ForMember(dest => dest.SlaDueAt, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore());

        CreateMap<Ticket, TicketResponseDto>()
            .ForMember(dest => dest.AssignedAgentId, opt => opt.MapFrom(src => src.AssignedToId))
            .ForMember(dest => dest.AssignedAgentName,
                opt => opt.MapFrom(src => src.AssignedTo != null ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}" : null))
            .ForMember(dest => dest.CreatedByName,
                opt => opt.MapFrom(src => src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : "Unknown"))
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.GetDisplayName()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.GetDisplayName()))
            .ForMember(dest => dest.AttachmentUrl, opt => opt.MapFrom(src => src.AttachmentUrl))
            .ForMember(dest => dest.IsBreached, opt => opt.MapFrom(src => IsBreached(src)))
            .ForMember(dest => dest.OverdueBy, opt => opt.MapFrom(src => FormatOverdue(src)));

        CreateMap<Category, CategoryResponseDto>();

        CreateMap<CreateCategoryDto, Category>();
    }

    private static bool IsBreached(Ticket ticket)
    {
        if (!ticket.SlaDueAt.HasValue)
            return false;

        if (ticket.ResolvedAt.HasValue)
            return ticket.ResolvedAt.Value > ticket.SlaDueAt.Value;

        if (ticket.Status == Domain.Enums.TicketStatus.Resolved || ticket.Status == Domain.Enums.TicketStatus.Closed)
            return false;

        return DateTime.UtcNow > ticket.SlaDueAt.Value;
    }

    private static string? FormatOverdue(Ticket ticket)
    {
        if (!ticket.SlaDueAt.HasValue || !IsBreached(ticket))
            return null;

        var referenceTime = ticket.ResolvedAt ?? DateTime.UtcNow;
        var overdue = referenceTime - ticket.SlaDueAt.Value;

        var days = (int)overdue.TotalDays;
        if (days >= 1)
            return $"{days} day{(days == 1 ? "" : "s")}";

        var hours = (int)overdue.TotalHours;
        if (hours >= 1)
            return $"{hours} hr{(hours == 1 ? "" : "s")}";

        var minutes = (int)overdue.TotalMinutes;
        return $"{minutes} min{(minutes == 1 ? "" : "s")}";
    }
}
