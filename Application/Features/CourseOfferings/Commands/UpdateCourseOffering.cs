using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.CourseOfferings.Dtos;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferings.Commands;

public record UpdateCourseOfferingCommand(int Id, string? Note) : IRequest<CourseOfferingDto>;

public class UpdateCourseOfferingCommandValidator : AbstractValidator<UpdateCourseOfferingCommand>
{
    public UpdateCourseOfferingCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class UpdateCourseOfferingCommandHandler : IRequestHandler<UpdateCourseOfferingCommand, CourseOfferingDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourseOfferingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseOfferingDto> Handle(UpdateCourseOfferingCommand request, CancellationToken cancellationToken)
    {
        var offering = await _context.CourseOfferings.FindAsync(request.Id, cancellationToken);
        if (offering == null) throw new NotFoundException(nameof(offering), request.Id);

        offering.Note = request.Note;
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.CourseOfferings
            .AsNoTracking()
            .Include(co => co.Course)
            .Include(co => co.Term)
            .FirstAsync(co => co.Id == offering.Id, cancellationToken);

        return result.Adapt<CourseOfferingDto>();
    }
}
