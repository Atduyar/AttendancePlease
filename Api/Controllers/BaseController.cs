using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    private IApplicationDbContext? _dbContext;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    protected IApplicationDbContext DbContext => _dbContext ??= HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();

    protected int CurrentUserId
    {
        get
        {
            var userId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.Identity?.Name;

            if (!int.TryParse(userId, out var parsedUserId))
                throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");

            return parsedUserId;
        }
    }

    protected async Task<bool> HasOfferingAccessAsync(
        int courseOfferingId,
        CourseOfferingStaffAccessLevel maxAccessLevel = CourseOfferingStaffAccessLevel.Viewer,
        CancellationToken cancellationToken = default,
        bool allowGlobalStaffRole = true)
    {
        if (User.IsInRole("Admin")) return true;
        if (allowGlobalStaffRole && User.IsInRole("Staff")) return true;
        if (!User.IsInRole("Student") && !User.IsInRole("Staff")) return false;

        return await DbContext.CourseOfferingStaffs.AnyAsync(s =>
            s.CourseOfferingId == courseOfferingId &&
            s.UserId == CurrentUserId &&
            s.Scope == CourseOfferingStaffScope.Offering &&
            s.AccessLevel <= maxAccessLevel,
            cancellationToken);
    }

    protected async Task<bool> HasSectionAccessAsync(
        int sectionId,
        CourseOfferingStaffAccessLevel maxAccessLevel = CourseOfferingStaffAccessLevel.Viewer,
        CancellationToken cancellationToken = default,
        bool allowGlobalStaffRole = true)
    {
        if (User.IsInRole("Admin")) return true;
        if (allowGlobalStaffRole && User.IsInRole("Staff")) return true;
        if (!User.IsInRole("Student") && !User.IsInRole("Staff")) return false;

        return await DbContext.Sections.AnyAsync(section =>
            section.Id == sectionId &&
            DbContext.CourseOfferingStaffs.Any(staff =>
                staff.UserId == CurrentUserId &&
                staff.AccessLevel <= maxAccessLevel &&
                (
                    (staff.Scope == CourseOfferingStaffScope.Offering && staff.CourseOfferingId == section.CourseOfferingId) ||
                    (staff.Scope == CourseOfferingStaffScope.Section && staff.SectionId == section.Id)
                )),
            cancellationToken);
    }

    protected async Task<bool> HasModuleAccessAsync(
        int moduleId,
        CourseOfferingStaffAccessLevel maxAccessLevel = CourseOfferingStaffAccessLevel.Viewer,
        CancellationToken cancellationToken = default,
        bool allowGlobalStaffRole = true)
    {
        if (User.IsInRole("Admin")) return true;
        if (allowGlobalStaffRole && User.IsInRole("Staff")) return true;
        if (!User.IsInRole("Student") && !User.IsInRole("Staff")) return false;

        return await DbContext.Modules.AnyAsync(module =>
            module.Id == moduleId &&
            DbContext.CourseOfferingStaffs.Any(staff =>
                staff.UserId == CurrentUserId &&
                staff.CourseOfferingId == module.CourseOfferingId &&
                staff.Scope == CourseOfferingStaffScope.Offering &&
                staff.AccessLevel <= maxAccessLevel),
            cancellationToken);
    }

    protected async Task<bool> HasSessionAccessAsync(
        int sessionId,
        CourseOfferingStaffAccessLevel maxAccessLevel = CourseOfferingStaffAccessLevel.Viewer,
        CancellationToken cancellationToken = default,
        bool allowGlobalStaffRole = true)
    {
        if (User.IsInRole("Admin")) return true;
        if (allowGlobalStaffRole && User.IsInRole("Staff")) return true;
        if (!User.IsInRole("Student") && !User.IsInRole("Staff")) return false;

        return await DbContext.Sessions.AnyAsync(session =>
            session.Id == sessionId &&
            DbContext.CourseOfferingStaffs.Any(staff =>
                staff.UserId == CurrentUserId &&
                staff.AccessLevel <= maxAccessLevel &&
                (
                    (staff.Scope == CourseOfferingStaffScope.Offering && staff.CourseOfferingId == session.Module.CourseOfferingId) ||
                    (staff.Scope == CourseOfferingStaffScope.Section && staff.SectionId == session.SectionId)
                )),
            cancellationToken);
    }

    protected async Task<bool> HasEnrollmentAccessAsync(
        int enrollmentId,
        CourseOfferingStaffAccessLevel maxAccessLevel = CourseOfferingStaffAccessLevel.Viewer,
        CancellationToken cancellationToken = default,
        bool allowGlobalStaffRole = true)
    {
        if (User.IsInRole("Admin")) return true;
        if (allowGlobalStaffRole && User.IsInRole("Staff")) return true;
        if (!User.IsInRole("Student") && !User.IsInRole("Staff")) return false;

        return await DbContext.Enrollments.AnyAsync(enrollment =>
            enrollment.Id == enrollmentId &&
            DbContext.CourseOfferingStaffs.Any(staff =>
                staff.UserId == CurrentUserId &&
                staff.AccessLevel <= maxAccessLevel &&
                (
                    (staff.Scope == CourseOfferingStaffScope.Offering && staff.CourseOfferingId == enrollment.CourseOfferingId) ||
                    (staff.Scope == CourseOfferingStaffScope.Section && staff.SectionId == enrollment.SectionId)
                )),
            cancellationToken);
    }
}
