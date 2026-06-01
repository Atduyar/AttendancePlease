using Domain.Common;

namespace Domain.Entities;

public class AttendanceSession : IBaseAuditableEntity
{
    public Guid Id { get; set; }
    public int CourseOfferingId { get; set; }
    public int CreatedByUserId { get; set; }
    public string SessionToken { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; } = 50;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public CourseOffering CourseOffering { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<AttendanceRecord> Records { get; set; } = [];
}
