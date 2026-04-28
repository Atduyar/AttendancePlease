using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Modules.Commands;

public record UpdateModuleCommand(int Id, string Title, int OrderIndex) : IRequest<bool>;

public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderIndex).GreaterThan(0);
    }
}

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateModuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FindAsync(new object[] { request.Id }, cancellationToken);
        if (module == null) return false;

        module.Title = request.Title;
        module.OrderIndex = request.OrderIndex;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
