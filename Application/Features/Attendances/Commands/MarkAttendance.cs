using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Attendances.Commands;

public record MarkAttendanceCommand(int UserId, int SessionId, AttendanceStatus Status, AttendanceMethod Method) : IRequest<int>;

public class MarkAttendanceCommandValidator : AbstractValidator<MarkAttendanceCommand>
{
    public MarkAttendanceCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
    }
}

public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, int>
{
    private readonly IApplicationDbContext _context;

    public MarkAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
    {
        var attendance = new Attendance
        {
            UserId = request.UserId,
            SessionId = request.SessionId,
            Status = request.Status,
            Method = request.Method,
            RecordedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);
        return attendance.Id;
    }
}
