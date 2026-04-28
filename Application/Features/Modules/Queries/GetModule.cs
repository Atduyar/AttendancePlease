using Application.Common.Interfaces;
using Application.Features.Modules.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Modules.Queries;

public record GetModuleQuery(int Id) : IRequest<ModuleDto?>;

public class GetModuleQueryValidator : AbstractValidator<GetModuleQuery>
{
    public GetModuleQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetModuleQueryHandler : IRequestHandler<GetModuleQuery, ModuleDto?>
{
    private readonly IApplicationDbContext _context;

    public GetModuleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModuleDto?> Handle(GetModuleQuery request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FindAsync(new object[] { request.Id }, cancellationToken);
        return module?.Adapt<ModuleDto>();
    }
}
