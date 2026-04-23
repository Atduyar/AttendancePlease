using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Sections.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Sections.Commands;

public record UpdateSectionCommand(int Id, string Name) : IRequest<SectionDto>;

public class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, SectionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SectionDto> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _context.Sections.FindAsync(request.Id, cancellationToken);
        if (section == null) throw new NotFoundException(nameof(section), request.Id);

        section.Name = request.Name;
        await _context.SaveChangesAsync(cancellationToken);
        return section.Adapt<SectionDto>();
    }
}
