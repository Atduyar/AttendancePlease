namespace Application.Features.Enrollments;

public static class StudentNumber
{
    private const string StudentEmailDomain = "@student.ius.edu.ba";

    public static string Normalize(string value) => value.Trim();

    public static string? FromStudentEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var normalized = email.Trim().ToLowerInvariant();
        if (!normalized.EndsWith(StudentEmailDomain, StringComparison.OrdinalIgnoreCase)) return null;

        var localPart = normalized[..^StudentEmailDomain.Length];
        return localPart.Length > 0 && localPart.All(char.IsDigit) ? localPart : null;
    }
}
