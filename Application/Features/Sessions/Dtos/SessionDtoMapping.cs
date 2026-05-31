using Domain.Entities;

namespace Application.Features.Sessions.Dtos;

public static class SessionDtoMapping
{
    public static SessionDto ToDto(Session s) => new(
        s.Id,
        s.ModuleId,
        s.Module.Title,
        s.SectionId,
        s.Section != null ? s.Section.Name : null,
        s.Status,
        s.SelectedMethod,
        s.OpenedByUserId,
        s.OpenedByUser.Name,
        s.OpenedAt,
        s.ClosedAt,
        s.CreatedAt,
        s.Latitude,
        s.Longitude,
        s.RadiusMeters);
}
