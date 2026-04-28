using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Modules.Commands;

public record DeleteModuleCommand(int Id) : IRequest<bool>;

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteModuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FindAsync(new object[] { request.Id }, cancellationToken);
        if (module == null) return false;

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
