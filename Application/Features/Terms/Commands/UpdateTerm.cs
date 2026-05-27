using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Terms.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Terms.Commands;

public record UpdateTermCommand(int Id, string Code, DateTime StartDate, DateTime EndDate) : IRequest<TermDto>;

public class UpdateTermCommandValidator : AbstractValidator<UpdateTermCommand>
{
    public UpdateTermCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public class UpdateTermCommandHandler : IRequestHandler<UpdateTermCommand, TermDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTermCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TermDto> Handle(UpdateTermCommand request, CancellationToken cancellationToken)
    {
        var term = await _context.Terms.FindAsync(request.Id, cancellationToken);
        if (term == null) throw new NotFoundException(nameof(term), request.Id);

        term.Code = request.Code;
        term.StartDate = DateOnly.FromDateTime(request.StartDate);
        term.EndDate = DateOnly.FromDateTime(request.EndDate);
        await _context.SaveChangesAsync(cancellationToken);
        return term.Adapt<TermDto>();
    }
}
