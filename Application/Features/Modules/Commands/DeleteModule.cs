using Application.Common.Exceptions;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Modules.Commands;

public record DeleteModuleCommand(int Id) : IRequest;

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteModuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FindAsync(request.Id, cancellationToken);
        if (module == null) throw new NotFoundException(nameof(module), request.Id);

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
