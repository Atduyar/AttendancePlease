using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.CourseOfferings.Dtos;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferings.Queries;

public record GetCourseOfferingQuery(int Id) : IRequest<CourseOfferingDto>;

public class GetCourseOfferingQueryValidator : AbstractValidator<GetCourseOfferingQuery>
{
    public GetCourseOfferingQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetCourseOfferingQueryHandler : IRequestHandler<GetCourseOfferingQuery, CourseOfferingDto>
{
    private readonly IApplicationDbContext _context;

    public GetCourseOfferingQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseOfferingDto> Handle(GetCourseOfferingQuery request, CancellationToken cancellationToken)
    {
        var offering = await _context.CourseOfferings
            .AsNoTracking()
            .Include(co => co.Course)
            .Include(co => co.Term)
            .FirstOrDefaultAsync(co => co.Id == request.Id, cancellationToken);

        if (offering == null) throw new NotFoundException(nameof(offering), request.Id);
        return offering.Adapt<CourseOfferingDto>();
    }
}
