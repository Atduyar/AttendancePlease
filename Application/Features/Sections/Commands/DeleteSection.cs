using Application.Common.Exceptions;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        var hasEnrollments = await _context.Enrollments.AnyAsync(x => x.SectionId == request.Id, cancellationToken);
        var hasSessions = await _context.Sessions.AnyAsync(x => x.SectionId == request.Id, cancellationToken);
        if (hasEnrollments || hasSessions)
        {
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                ["Section"] = ["Cannot delete a section that has enrolled students or sessions. Move students and keep historical sessions before deleting."]
            });
        }

        _context.Sections.Remove(section);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
