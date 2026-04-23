using Domain.Enums;

namespace Application.Features.Sessions.Dtos;

public record SessionDto(
    int Id,
    int ModuleId,
    string ModuleTitle,
    int? SectionId,
    string? SectionName,
    SessionStatus Status,
    AttendanceMethod SelectedMethod,
    int OpenedByUserId,
    string OpenedByUserName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    DateTime CreatedAt);
