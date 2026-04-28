using Application.Common.Interfaces;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Sections.Commands;

public record CreateSectionCommand(int CourseOfferingId, string Name) : IRequest<int>;

public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = request.Adapt<Section>();
        _context.Sections.Add(section);
        await _context.SaveChangesAsync(cancellationToken);
        return section.Id;
    }
}
