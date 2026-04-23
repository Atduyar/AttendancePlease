using Application.Common.Exceptions;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Terms.Commands;

public record DeleteTermCommand(int Id) : IRequest;

public class DeleteTermCommandValidator : AbstractValidator<DeleteTermCommand>
{
    public DeleteTermCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteTermCommandHandler : IRequestHandler<DeleteTermCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTermCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteTermCommand request, CancellationToken cancellationToken)
    {
        var term = await _context.Terms.FindAsync(request.Id, cancellationToken);
        if (term == null) throw new NotFoundException(nameof(term), request.Id);

        _context.Terms.Remove(term);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
