using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Terms.Commands;

public record UpdateTermCommand(int Id, string Code, DateOnly StartDate, DateOnly EndDate) : IRequest<bool>;

public class UpdateTermCommandValidator : AbstractValidator<UpdateTermCommand>
{
    public UpdateTermCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public class UpdateTermCommandHandler : IRequestHandler<UpdateTermCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateTermCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateTermCommand request, CancellationToken cancellationToken)
    {
        var term = await _context.Terms.FindAsync(new object[] { request.Id }, cancellationToken);
        if (term == null) return false;

        term.Code = request.Code;
        term.StartDate = request.StartDate;
        term.EndDate = request.EndDate;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
