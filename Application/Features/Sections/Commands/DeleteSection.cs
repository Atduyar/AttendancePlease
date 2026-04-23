using Application.Common.Exceptions;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Sections.Commands;

public record DeleteSectionCommand(int Id) : IRequest;

public class DeleteSectionCommandValidator : AbstractValidator<DeleteSectionCommand>
{
    public DeleteSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.Sections.FindAsync(request.Id, cancellationToken);
        if (section == null) throw new NotFoundException(nameof(section), request.Id);

        _context.Sections.Remove(section);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
