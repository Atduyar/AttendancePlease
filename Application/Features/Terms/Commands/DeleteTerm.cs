using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Terms.Commands;

public record DeleteTermCommand(int Id) : IRequest<bool>;

public class DeleteTermCommandValidator : AbstractValidator<DeleteTermCommand>
{
    public DeleteTermCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteTermCommandHandler : IRequestHandler<DeleteTermCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTermCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTermCommand request, CancellationToken cancellationToken)
    {
        var term = await _context.Terms.FindAsync(new object[] { request.Id }, cancellationToken);
        if (term == null) return false;

        _context.Terms.Remove(term);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
