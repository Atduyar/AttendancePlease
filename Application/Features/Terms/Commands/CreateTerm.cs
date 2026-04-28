using Application.Common.Interfaces;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Terms.Commands;

public record CreateTermCommand(string Code, DateOnly StartDate, DateOnly EndDate) : IRequest<int>;

public class CreateTermCommandValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public class CreateTermCommandHandler : IRequestHandler<CreateTermCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateTermCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateTermCommand request, CancellationToken cancellationToken)
    {
        var term = request.Adapt<Term>();
        _context.Terms.Add(term);
        await _context.SaveChangesAsync(cancellationToken);
        return term.Id;
    }
}
