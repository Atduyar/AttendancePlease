using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Sections.Commands;

public record DeleteSectionCommand(int Id) : IRequest<bool>;

public class DeleteSectionCommandValidator : AbstractValidator<DeleteSectionCommand>
{
    public DeleteSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.Sections.FindAsync(new object[] { request.Id }, cancellationToken);
        if (section == null) return false;

        _context.Sections.Remove(section);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
