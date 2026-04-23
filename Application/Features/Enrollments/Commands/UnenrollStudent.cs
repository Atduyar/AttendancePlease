using Application.Common.Exceptions;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Enrollments.Commands;

public record UnenrollStudentCommand(int Id) : IRequest;

public class UnenrollStudentCommandValidator : AbstractValidator<UnenrollStudentCommand>
{
    public UnenrollStudentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class UnenrollStudentCommandHandler : IRequestHandler<UnenrollStudentCommand>
{
    private readonly IApplicationDbContext _context;

    public UnenrollStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UnenrollStudentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.Enrollments.FindAsync(request.Id, cancellationToken);
        if (enrollment == null) throw new NotFoundException(nameof(enrollment), request.Id);

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
