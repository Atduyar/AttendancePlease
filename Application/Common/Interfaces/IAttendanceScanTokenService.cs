namespace Application.Common.Interfaces;

public interface IAttendanceScanTokenService
{
    AttendanceScanTokenIssueResult Issue(int sessionId);
    AttendanceScanTokenValidationResult Validate(string token);
}

public record AttendanceScanTokenIssueResult(string Token, DateTime ExpiresAt);

public record AttendanceScanTokenValidationResult(bool IsValid, int? SessionId, string? Error);
