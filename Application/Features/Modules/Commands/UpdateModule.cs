using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Modules.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Modules.Commands;

public record UpdateModuleCommand(int Id, string Title, int OrderIndex) : IRequest<ModuleDto>;

public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderIndex).GreaterThan(0);
    }
}

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, ModuleDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateModuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModuleDto> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FindAsync(request.Id, cancellationToken);
        if (module == null) throw new NotFoundException(nameof(module), request.Id);

        module.Title = request.Title;
        module.OrderIndex = request.OrderIndex;
        await _context.SaveChangesAsync(cancellationToken);
        return module.Adapt<ModuleDto>();
    }
}
