namespace Application.Features.Terms.Dtos;

public record TermDto(int Id, string Code, DateOnly StartDate, DateOnly EndDate, DateTime CreatedAt);
