namespace Domain.Entities;

public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int UserId { get; set; }
    public string UserDisplayName { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceMeters { get; set; }
    public bool IsApproved { get; set; }
    public DateTime RecordedAt { get; set; }

    public AttendanceSession Session { get; set; } = null!;
    public User User { get; set; } = null!;
}
