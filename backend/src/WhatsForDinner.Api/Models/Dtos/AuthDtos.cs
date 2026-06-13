namespace WhatsForDinner.Api.Models.Dtos;

public record GoogleSignInRequest(string Credential);

public record AuthUserDto(
    int Id,
    string Email,
    string DisplayName,
    string? AvatarUrl
);