using Application.Common.Interfaces;
using Application.Features.Courses.Dtos;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Courses.Queries;

public record ListCoursesQuery : IRequest<List<CourseDto>>;

public class ListCoursesQueryHandler : IRequestHandler<ListCoursesQuery, List<CourseDto>>
{
    private readonly IApplicationDbContext _context;

    public ListCoursesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseDto>> Handle(ListCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return courses.Adapt<List<CourseDto>>();
    }
}
