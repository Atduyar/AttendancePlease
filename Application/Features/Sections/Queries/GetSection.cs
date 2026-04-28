using Application.Common.Interfaces;
using Application.Features.Sections.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Sections.Queries;

public record GetSectionQuery(int Id) : IRequest<SectionDto?>;

public class GetSectionQueryValidator : AbstractValidator<GetSectionQuery>
{
    public GetSectionQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetSectionQueryHandler : IRequestHandler<GetSectionQuery, SectionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSectionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SectionDto?> Handle(GetSectionQuery request, CancellationToken cancellationToken)
    {
        var section = await _context.Sections.FindAsync(new object[] { request.Id }, cancellationToken);
        return section?.Adapt<SectionDto>();
    }
}
