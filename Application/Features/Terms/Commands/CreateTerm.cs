using Application.Common.Interfaces;
using Application.Features.Terms.Dtos;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Terms.Commands;

public record CreateTermCommand(string Name, DateTime StartDate, DateTime EndDate) : IRequest<TermDto>;

public class CreateTermCommandValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public class CreateTermCommandHandler : IRequestHandler<CreateTermCommand, TermDto>
{
    private readonly IApplicationDbContext _context;

    public CreateTermCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TermDto> Handle(CreateTermCommand request, CancellationToken cancellationToken)
    {
        var term = request.Adapt<Term>();
        _context.Terms.Add(term);
        await _context.SaveChangesAsync(cancellationToken);
        return term.Adapt<TermDto>();
    }
}
