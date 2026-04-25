using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Attendance : BaseEntity
{
    public int UserId { get; set; }
    public int SessionId { get; set; }
    public AttendanceStatus Status { get; set; }
    public AttendanceMethod Method { get; set; }
    public DateTime? RecordedAt { get; set; }

    public User User { get; set; } = null!;
    public Session Session { get; set; } = null!;
}
