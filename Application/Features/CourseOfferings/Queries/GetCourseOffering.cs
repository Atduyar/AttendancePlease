using Application.Common.Interfaces;
using Application.Features.CourseOfferings.Dtos;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferings.Queries;

public record GetCourseOfferingQuery(int Id) : IRequest<CourseOfferingDto?>;

public class GetCourseOfferingQueryValidator : AbstractValidator<GetCourseOfferingQuery>
{
    public GetCourseOfferingQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetCourseOfferingQueryHandler : IRequestHandler<GetCourseOfferingQuery, CourseOfferingDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCourseOfferingQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseOfferingDto?> Handle(GetCourseOfferingQuery request, CancellationToken cancellationToken)
    {
        var offering = await _context.CourseOfferings
            .AsNoTracking()
            .Include(o => o.Course)
            .Include(o => o.Term)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (offering == null) return null;

        return new CourseOfferingDto(
            offering.Id,
            offering.CourseId,
            offering.Course.Code,
            offering.Course.Title,
            offering.TermId,
            offering.Term.Code,
            offering.Note,
            offering.CreatedAt);
    }
}
