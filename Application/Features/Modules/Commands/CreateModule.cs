using Application.Common.Interfaces;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Modules.Commands;

public record CreateModuleCommand(int CourseOfferingId, string Title, int OrderIndex) : IRequest<int>;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderIndex).GreaterThan(0);
    }
}

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateModuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = request.Adapt<Module>();
        _context.Modules.Add(module);
        await _context.SaveChangesAsync(cancellationToken);
        return module.Id;
    }
}
