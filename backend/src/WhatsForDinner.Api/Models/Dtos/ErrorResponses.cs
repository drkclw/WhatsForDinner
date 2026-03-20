namespace WhatsForDinner.Api.Models.Dtos;

public record ErrorResponse(string Message);

public record ValidationErrorResponse(
    string Message,
    IDictionary<string, string[]> Errors
);
