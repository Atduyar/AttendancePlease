using Application.Common.Interfaces;
using Application.Features.Attendances.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Queries;

public record ListAttendancesBySessionQuery(int SessionId) : IRequest<List<AttendanceDto>>;

public class ListAttendancesBySessionQueryValidator : AbstractValidator<ListAttendancesBySessionQuery>
{
    public ListAttendancesBySessionQueryValidator()
    {
        RuleFor(x => x.SessionId).GreaterThan(0);
    }
}

public class ListAttendancesBySessionQueryHandler : IRequestHandler<ListAttendancesBySessionQuery, List<AttendanceDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAttendancesBySessionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AttendanceDto>> Handle(ListAttendancesBySessionQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _context.Attendances
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Session)
            .ThenInclude(s => s.Module)
            .Where(a => a.SessionId == request.SessionId)
            .ToListAsync(cancellationToken);

        return attendances.Select(a => new AttendanceDto(
            a.Id,
            a.UserId,
            a.User.Name,
            a.SessionId,
            a.Session.Module.Title,
            a.Status,
            a.Method,
            a.RecordedAt,
            a.CreatedAt)).ToList();
    }
}
