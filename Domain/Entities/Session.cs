using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Session : BaseEntity
{
    public int ModuleId { get; set; }
    public int? SectionId { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Open;
    public AttendanceMethod SelectedMethod { get; set; }

    public int OpenedByUserId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Module Module { get; set; } = null!;
    public Section? Section { get; set; }
    public User OpenedByUser { get; set; } = null!;
    public ICollection<Attendance> Attendances { get; set; } = [];
}
