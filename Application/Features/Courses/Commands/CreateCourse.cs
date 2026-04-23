using Application.Common.Interfaces;
using Application.Features.Courses.Dtos;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Courses.Commands;

public record CreateCourseCommand(string Code, string Title, string? Description) : IRequest<CourseDto>;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = request.Adapt<Course>();
        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);
        return course.Adapt<CourseDto>();
    }
}
