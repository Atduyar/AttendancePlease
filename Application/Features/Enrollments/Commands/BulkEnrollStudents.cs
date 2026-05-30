using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Enrollments.Commands;

public record BulkEnrollStudentItem(string StudentNumber, string? ImportedName, int SectionId);

public record BulkEnrollStudentsCommand(int CourseOfferingId, List<BulkEnrollStudentItem> Students) : IRequest<BulkEnrollStudentsResult>;

public record BulkEnrollStudentResult(string StudentNumber, bool Success, bool LinkedUser, string Message);

public record BulkEnrollStudentsResult(int SuccessCount, int ErrorCount, List<BulkEnrollStudentResult> Results);

public class BulkEnrollStudentsCommandValidator : AbstractValidator<BulkEnrollStudentsCommand>
{
    public BulkEnrollStudentsCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.Students).NotEmpty();
        RuleForEach(x => x.Students).ChildRules(item =>
        {
            item.RuleFor(x => x.StudentNumber).NotEmpty().MaximumLength(64);
            item.RuleFor(x => x.SectionId).GreaterThan(0);
            item.RuleFor(x => x.ImportedName).MaximumLength(200);
        });
    }
}

public class BulkEnrollStudentsCommandHandler : IRequestHandler<BulkEnrollStudentsCommand, BulkEnrollStudentsResult>
{
    private readonly IApplicationDbContext _context;

    public BulkEnrollStudentsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BulkEnrollStudentsResult> Handle(BulkEnrollStudentsCommand request, CancellationToken cancellationToken)
    {
        var sectionIds = request.Students.Select(s => s.SectionId).Distinct().ToList();
        var validSectionIds = await _context.Sections
            .Where(s => s.CourseOfferingId == request.CourseOfferingId && sectionIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);
        var validSectionSet = validSectionIds.ToHashSet();

        var normalizedNumbers = request.Students
            .Select(s => StudentNumber.Normalize(s.StudentNumber))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        var existingEnrollments = await _context.Enrollments
            .Where(e => e.CourseOfferingId == request.CourseOfferingId && normalizedNumbers.Contains(e.StudentNumber))
            .Select(e => e.StudentNumber)
            .ToListAsync(cancellationToken);
        var existingEnrollmentSet = existingEnrollments.ToHashSet();

        var usersByStudentNumber = await _context.Users
            .Where(u => u.StudentNumber != null && normalizedNumbers.Contains(u.StudentNumber))
            .ToDictionaryAsync(u => u.StudentNumber!, cancellationToken);

        var seenInFile = new HashSet<string>();
        var results = new List<BulkEnrollStudentResult>();

        foreach (var item in request.Students)
        {
            var number = StudentNumber.Normalize(item.StudentNumber);
            if (string.IsNullOrWhiteSpace(number))
            {
                results.Add(new BulkEnrollStudentResult(item.StudentNumber, false, false, "Student number is required."));
                continue;
            }

            if (!validSectionSet.Contains(item.SectionId))
            {
                results.Add(new BulkEnrollStudentResult(number, false, false, "Section does not belong to this course offering."));
                continue;
            }

            if (!seenInFile.Add(number))
            {
                results.Add(new BulkEnrollStudentResult(number, false, false, "Duplicate student number in import file."));
                continue;
            }

            if (existingEnrollmentSet.Contains(number))
            {
                results.Add(new BulkEnrollStudentResult(number, false, usersByStudentNumber.ContainsKey(number), "Already enrolled in this course offering."));
                continue;
            }

            usersByStudentNumber.TryGetValue(number, out var user);
            _context.Enrollments.Add(new Domain.Entities.Enrollment
            {
                UserId = user?.Id,
                StudentNumber = number,
                ImportedName = string.IsNullOrWhiteSpace(item.ImportedName) ? null : item.ImportedName.Trim(),
                CourseOfferingId = request.CourseOfferingId,
                SectionId = item.SectionId
            });
            existingEnrollmentSet.Add(number);
            results.Add(new BulkEnrollStudentResult(number, true, user != null, user != null ? "Enrolled and linked to registered user." : "Enrolled pending first login."));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BulkEnrollStudentsResult(
            results.Count(r => r.Success),
            results.Count(r => !r.Success),
            results);
    }
}
