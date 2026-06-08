using WhatsForDinner.Api.Models;

namespace WhatsForDinner.Api.Services;

public interface IAuthService
{
    Task<User> SignInWithGoogleAsync(string credential, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    string CreateSessionToken(User user);
}