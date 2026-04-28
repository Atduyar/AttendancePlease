using Application.Common.Interfaces;
using Application.Features.Attendances.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Queries;

public record GetAttendanceQuery(int Id) : IRequest<AttendanceDto?>;

public class GetAttendanceQueryValidator : AbstractValidator<GetAttendanceQuery>
{
    public GetAttendanceQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetAttendanceQueryHandler : IRequestHandler<GetAttendanceQuery, AttendanceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceDto?> Handle(GetAttendanceQuery request, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Session)
            .ThenInclude(s => s.Module)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (attendance == null) return null;

        return new AttendanceDto(
            attendance.Id,
            attendance.UserId,
            attendance.User.Name,
            attendance.SessionId,
            attendance.Session.Module.Title,
            attendance.Status,
            attendance.Method,
            attendance.RecordedAt,
            attendance.CreatedAt);
    }
}
