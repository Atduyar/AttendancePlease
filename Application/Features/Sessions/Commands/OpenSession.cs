using Application.Common.Interfaces;
using Application.Features.Sessions.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Commands;

public record OpenSessionCommand(int ModuleId, int? SectionId, AttendanceMethod SelectedMethod, int OpenedByUserId) : IRequest<SessionDto>;

public class OpenSessionCommandValidator : AbstractValidator<OpenSessionCommand>
{
    public OpenSessionCommandValidator()
    {
        RuleFor(x => x.ModuleId).GreaterThan(0);
        RuleFor(x => x.OpenedByUserId).GreaterThan(0);
    }
}

public class OpenSessionCommandHandler : IRequestHandler<OpenSessionCommand, SessionDto>
{
    private readonly IApplicationDbContext _context;

    public OpenSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SessionDto> Handle(OpenSessionCommand request, CancellationToken cancellationToken)
    {
        if (request.SectionId.HasValue)
        {
            var sectionMatchesModuleOffering = await _context.Sections.AnyAsync(
                section => section.Id == request.SectionId.Value &&
                    _context.Modules.Any(module => module.Id == request.ModuleId && module.CourseOfferingId == section.CourseOfferingId),
                cancellationToken);
            if (!sectionMatchesModuleOffering)
            {
                throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
                {
                    [nameof(request.SectionId)] = ["Section must belong to the module course offering."]
                });
            }
        }

        var session = new Session
        {
            ModuleId = request.ModuleId,
            SectionId = request.SectionId,
            Status = SessionStatus.Open,
            SelectedMethod = request.SelectedMethod,
            OpenedByUserId = request.OpenedByUserId,
            OpenedAt = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Module)
            .Include(s => s.Section)
            .Include(s => s.OpenedByUser)
            .FirstAsync(s => s.Id == session.Id, cancellationToken);

        return new SessionDto(
            result.Id,
            result.ModuleId,
            result.Module.Title,
            result.SectionId,
            result.Section?.Name,
            result.Status,
            result.SelectedMethod,
            result.OpenedByUserId,
            result.OpenedByUser.Name,
            result.OpenedAt,
            result.ClosedAt,
            result.CreatedAt);
    }
}
