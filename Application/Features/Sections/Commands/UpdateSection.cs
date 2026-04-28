using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Sections.Commands;

public record UpdateSectionCommand(int Id, string Name) : IRequest<bool>;

public class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.Sections.FindAsync(new object[] { request.Id }, cancellationToken);
        if (section == null) return false;

        section.Name = request.Name;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
