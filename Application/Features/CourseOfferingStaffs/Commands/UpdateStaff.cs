using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.CourseOfferingStaffs.Dtos;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferingStaffs.Commands;

public record UpdateStaffCommand(
    int Id,
    CourseOfferingStaffScope Scope,
    CourseOfferingStaffAccessLevel AccessLevel,
    int? SectionId,
    string? RoleTitle) : IRequest<CourseOfferingStaffDto>;

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.AccessLevel).IsInEnum();
        RuleFor(x => x.SectionId).NotNull().GreaterThan(0).When(x => x.Scope == CourseOfferingStaffScope.Section);
        RuleFor(x => x.SectionId).Null().When(x => x.Scope == CourseOfferingStaffScope.Offering);
        RuleFor(x => x.RoleTitle).MaximumLength(100);
    }
}

public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, CourseOfferingStaffDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseOfferingStaffDto> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.CourseOfferingStaffs.FindAsync(request.Id, cancellationToken);
        if (assignment == null) throw new NotFoundException(nameof(assignment), request.Id);

        if (request.Scope == CourseOfferingStaffScope.Section)
        {
            var sectionBelongsToOffering = await _context.Sections.AnyAsync(
                s => s.Id == request.SectionId && s.CourseOfferingId == assignment.CourseOfferingId,
                cancellationToken);
            if (!sectionBelongsToOffering)
            {
                throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
                {
                    [nameof(request.SectionId)] = ["Section must belong to the selected course offering."]
                });
            }
        }

        var duplicateExists = await _context.CourseOfferingStaffs.AnyAsync(s =>
            s.Id != request.Id &&
            s.CourseOfferingId == assignment.CourseOfferingId &&
            s.UserId == assignment.UserId &&
            s.Scope == request.Scope &&
            s.SectionId == request.SectionId,
            cancellationToken);
        if (duplicateExists)
        {
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.Id)] = ["This user already has this assignment."]
            });
        }

        assignment.Scope = request.Scope;
        assignment.AccessLevel = request.AccessLevel;
        assignment.SectionId = request.SectionId;
        assignment.RoleTitle = request.RoleTitle;

        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.CourseOfferingStaffs
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Section)
            .FirstAsync(s => s.Id == request.Id, cancellationToken);

        return new CourseOfferingStaffDto(
            result.Id,
            result.CourseOfferingId,
            result.SectionId,
            result.Section?.Name,
            result.UserId,
            result.User.Name,
            result.User.Email!,
            result.User.Role.ToString(),
            result.Scope,
            result.AccessLevel,
            result.RoleTitle,
            result.CreatedAt);
    }
}
