using Application.Common.Interfaces;
using Application.Features.Enrollments.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Enrollments.Queries;

public record GetEnrollmentQuery(int Id) : IRequest<EnrollmentDto?>;

public class GetEnrollmentQueryValidator : AbstractValidator<GetEnrollmentQuery>
{
    public GetEnrollmentQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetEnrollmentQueryHandler : IRequestHandler<GetEnrollmentQuery, EnrollmentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetEnrollmentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnrollmentDto?> Handle(GetEnrollmentQuery request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Section)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (enrollment == null) return null;

        return new EnrollmentDto(
            enrollment.Id,
            enrollment.UserId,
            enrollment.User.Name,
            enrollment.CourseOfferingId,
            enrollment.SectionId,
            enrollment.Section.Name,
            enrollment.CreatedAt);
    }
}
