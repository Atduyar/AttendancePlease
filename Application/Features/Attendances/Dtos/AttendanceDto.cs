using Domain.Enums;

namespace Application.Features.Attendances.Dtos;

public record AttendanceDto(
    int Id,
    int UserId,
    string UserName,
    int SessionId,
    string ModuleTitle,
    AttendanceStatus Status,
    AttendanceMethod Method,
    DateTime? RecordedAt,
    DateTime CreatedAt);
