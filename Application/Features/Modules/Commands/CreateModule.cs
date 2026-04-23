using Application.Common.Interfaces;
using Application.Features.Modules.Dtos;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Modules.Commands;

public record CreateModuleCommand(int CourseOfferingId, string Title, int OrderIndex) : IRequest<ModuleDto>;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderIndex).GreaterThan(0);
    }
}

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, ModuleDto>
{
    private readonly IApplicationDbContext _context;

    public CreateModuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModuleDto> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = request.Adapt<Module>();
        _context.Modules.Add(module);
        await _context.SaveChangesAsync(cancellationToken);
        return module.Adapt<ModuleDto>();
    }
}
