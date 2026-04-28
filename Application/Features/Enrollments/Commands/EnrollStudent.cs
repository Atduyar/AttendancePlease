using Application.Common.Interfaces;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.Enrollments.Commands;

public record EnrollStudentCommand(int UserId, int CourseOfferingId, int SectionId) : IRequest<int>;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.SectionId).GreaterThan(0);
    }
}

public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, int>
{
    private readonly IApplicationDbContext _context;

    public EnrollStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = new Enrollment
        {
            UserId = request.UserId,
            CourseOfferingId = request.CourseOfferingId,
            SectionId = request.SectionId
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
        return enrollment.Id;
    }
}
