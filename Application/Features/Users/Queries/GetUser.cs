using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Users.Dtos;
using FluentValidation;
using Mapster;
using MediatR;

namespace Application.Features.Users.Queries;

public record GetUserQuery(int Id) : IRequest<UserDto>;

public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(request.Id, cancellationToken);
        if (user == null) throw new NotFoundException(nameof(user), request.Id);
        return user.Adapt<UserDto>();
    }
}
