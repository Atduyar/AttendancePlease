using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Enrollments.Commands;

public record UnenrollStudentCommand(int Id) : IRequest<bool>;

public class UnenrollStudentCommandValidator : AbstractValidator<UnenrollStudentCommand>
{
    public UnenrollStudentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class UnenrollStudentCommandHandler : IRequestHandler<UnenrollStudentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UnenrollStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UnenrollStudentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.Enrollments.FindAsync(new object[] { request.Id }, cancellationToken);
        if (enrollment == null) return false;

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
