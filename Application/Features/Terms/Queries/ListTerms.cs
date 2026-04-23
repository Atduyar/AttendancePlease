using Application.Common.Interfaces;
using Application.Features.Terms.Dtos;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Terms.Queries;

public record ListTermsQuery : IRequest<List<TermDto>>;

public class ListTermsQueryHandler : IRequestHandler<ListTermsQuery, List<TermDto>>
{
    private readonly IApplicationDbContext _context;

    public ListTermsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TermDto>> Handle(ListTermsQuery request, CancellationToken cancellationToken)
    {
        var terms = await _context.Terms
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return terms.Adapt<List<TermDto>>();
    }
}
