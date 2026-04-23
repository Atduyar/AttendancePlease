using Application.Common.Interfaces;
using Application.Features.CourseOfferings.Dtos;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferings.Commands;

public record CreateCourseOfferingCommand(int CourseId, int TermId, string? Note) : IRequest<CourseOfferingDto>;

public class CreateCourseOfferingCommandValidator : AbstractValidator<CreateCourseOfferingCommand>
{
    public CreateCourseOfferingCommandValidator()
    {
        RuleFor(x => x.CourseId).GreaterThan(0);
        RuleFor(x => x.TermId).GreaterThan(0);
    }
}

public class CreateCourseOfferingCommandHandler : IRequestHandler<CreateCourseOfferingCommand, CourseOfferingDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCourseOfferingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseOfferingDto> Handle(CreateCourseOfferingCommand request, CancellationToken cancellationToken)
    {
        var offering = request.Adapt<CourseOffering>();
        _context.CourseOfferings.Add(offering);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.CourseOfferings
            .AsNoTracking()
            .Include(co => co.Course)
            .Include(co => co.Term)
            .FirstAsync(co => co.Id == offering.Id, cancellationToken);

        return result.Adapt<CourseOfferingDto>();
    }
}
