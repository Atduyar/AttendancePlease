using Application.Features.Terms.Dtos;
using Domain.Entities;
using FluentValidation;
using Mapster;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Terms.Queries;

public record GetTermQuery(int Id) : IRequest<TermDto?>;

public class GetTermQueryValidator : AbstractValidator<GetTermQuery>
{
    public GetTermQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetTermQueryHandler : IRequestHandler<GetTermQuery, TermDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTermQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TermDto?> Handle(GetTermQuery request, CancellationToken cancellationToken)
    {
        var term = await _context.Terms.FindAsync(new object[] { request.Id }, cancellationToken);
        return term?.Adapt<TermDto>();
    }
}
