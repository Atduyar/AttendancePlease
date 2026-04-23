using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Courses.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Courses.Commands;

public record UpdateCourseCommand(int Id, string Code, string Title, string? Description) : IRequest<CourseDto>;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDto> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync(request.Id, cancellationToken);
        if (course == null) throw new NotFoundException(nameof(course), request.Id);

        course.Code = request.Code;
        course.Title = request.Title;
        await _context.SaveChangesAsync(cancellationToken);
        return course.Adapt<CourseDto>();
    }
}
