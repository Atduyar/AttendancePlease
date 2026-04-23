using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Courses.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Courses.Queries;

public record GetCourseQuery(int Id) : IRequest<CourseDto>;

public class GetCourseQueryValidator : AbstractValidator<GetCourseQuery>
{
    public GetCourseQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, CourseDto>
{
    private readonly IApplicationDbContext _context;

    public GetCourseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDto> Handle(GetCourseQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync(request.Id, cancellationToken);
        if (course == null) throw new NotFoundException(nameof(course), request.Id);
        return course.Adapt<CourseDto>();
    }
}
