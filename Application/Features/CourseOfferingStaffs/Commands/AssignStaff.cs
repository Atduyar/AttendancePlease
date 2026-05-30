using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.CourseOfferingStaffs.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferingStaffs.Commands;

public record AssignStaffCommand(
    int CourseOfferingId,
    int UserId,
    CourseOfferingStaffScope Scope,
    CourseOfferingStaffAccessLevel AccessLevel,
    int? SectionId,
    string? RoleTitle) : IRequest<CourseOfferingStaffDto>;

public class AssignStaffCommandValidator : AbstractValidator<AssignStaffCommand>
{
    public AssignStaffCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.AccessLevel).IsInEnum();
        RuleFor(x => x.SectionId).NotNull().GreaterThan(0).When(x => x.Scope == CourseOfferingStaffScope.Section);
        RuleFor(x => x.SectionId).Null().When(x => x.Scope == CourseOfferingStaffScope.Offering);
        RuleFor(x => x.RoleTitle).MaximumLength(100);
    }
}

public class AssignStaffCommandHandler : IRequestHandler<AssignStaffCommand, CourseOfferingStaffDto>
{
    private readonly IApplicationDbContext _context;

    public AssignStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseOfferingStaffDto> Handle(AssignStaffCommand request, CancellationToken cancellationToken)
    {
        var offeringExists = await _context.CourseOfferings.AnyAsync(o => o.Id == request.CourseOfferingId, cancellationToken);
        if (!offeringExists) throw new NotFoundException("CourseOffering", request.CourseOfferingId);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) throw new NotFoundException("User", request.UserId);

        if (request.Scope == CourseOfferingStaffScope.Section)
        {
            var sectionBelongsToOffering = await _context.Sections.AnyAsync(
                s => s.Id == request.SectionId && s.CourseOfferingId == request.CourseOfferingId,
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
            s.CourseOfferingId == request.CourseOfferingId &&
            s.UserId == request.UserId &&
            s.Scope == request.Scope &&
            s.SectionId == request.SectionId,
            cancellationToken);
        if (duplicateExists)
        {
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.UserId)] = ["This user already has this assignment."]
            });
        }

        var assignment = new CourseOfferingStaff
        {
            CourseOfferingId = request.CourseOfferingId,
            UserId = request.UserId,
            Scope = request.Scope,
            AccessLevel = request.AccessLevel,
            SectionId = request.SectionId,
            RoleTitle = request.RoleTitle
        };

        _context.CourseOfferingStaffs.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.CourseOfferingStaffs
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Section)
            .FirstAsync(s => s.Id == assignment.Id, cancellationToken);

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
