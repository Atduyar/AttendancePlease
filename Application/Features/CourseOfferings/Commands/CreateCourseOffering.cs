using Application.Common.Interfaces;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.CourseOfferings.Commands;

public record CreateCourseOfferingCommand(int CourseId, int TermId, string? Note) : IRequest<int>;

public class CreateCourseOfferingCommandValidator : AbstractValidator<CreateCourseOfferingCommand>
{
    public CreateCourseOfferingCommandValidator()
    {
        RuleFor(x => x.CourseId).GreaterThan(0);
        RuleFor(x => x.TermId).GreaterThan(0);
    }
}

public class CreateCourseOfferingCommandHandler : IRequestHandler<CreateCourseOfferingCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCourseOfferingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCourseOfferingCommand request, CancellationToken cancellationToken)
    {
        var offering = request.Adapt<CourseOffering>();
        _context.CourseOfferings.Add(offering);
        await _context.SaveChangesAsync(cancellationToken);
        return offering.Id;
    }
}
