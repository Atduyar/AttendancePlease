using Application.Common.Interfaces;
using Application.Features.Attendances.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Commands;

public record MarkAttendanceCommand(int UserId, int SessionId, AttendanceStatus Status, AttendanceMethod Method) : IRequest<AttendanceDto>;

public class MarkAttendanceCommandValidator : AbstractValidator<MarkAttendanceCommand>
{
    public MarkAttendanceCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
    }
}

public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, AttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public MarkAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceDto> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
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

        var result = await _context.Attendances
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Session)
            .ThenInclude(s => s.Module)
            .FirstAsync(a => a.Id == attendance.Id, cancellationToken);

        return new AttendanceDto(
            result.Id,
            result.UserId,
            result.User.Name,
            result.SessionId,
            result.Session.Module.Title,
            result.Status,
            result.Method,
            result.RecordedAt,
            result.CreatedAt);
    }
}
