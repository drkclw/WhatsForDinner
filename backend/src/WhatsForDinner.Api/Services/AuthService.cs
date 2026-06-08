using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models;
using WhatsForDinner.Api.Models.Dtos;

namespace WhatsForDinner.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly AuthOptions _authOptions;

    public AuthService(ApplicationDbContext context, IOptions<AuthOptions> authOptions)
    {
        _context = context;
        _authOptions = authOptions.Value;
    }

    public async Task<User> SignInWithGoogleAsync(string credential, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            throw new ArgumentException("Google credential is required.", nameof(credential));
        }

        if (string.IsNullOrWhiteSpace(_authOptions.Google.ClientId))
        {
            throw new InvalidOperationException("Google authentication is not configured.");
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(
            credential,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_authOptions.Google.ClientId]
            });

        var now = DateTime.UtcNow;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == payload.Subject, cancellationToken);

        if (user == null)
        {
            user = new User
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                DisplayName = payload.Name ?? payload.Email,
                PictureUrl = payload.Picture,
                CreatedAt = now,
                LastLoginAt = now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Ensure new users have a weekly plan scaffold.
            _context.WeeklyPlans.Add(new WeeklyPlan
            {
                UserId = user.Id,
                CreatedAt = now,
                UpdatedAt = now
            });
            await _context.SaveChangesAsync(cancellationToken);

            return user;
        }

        user.Email = payload.Email;
        user.DisplayName = payload.Name ?? payload.Email;
        user.PictureUrl = payload.Picture;
        user.LastLoginAt = now;

        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public string CreateSessionToken(User user)
    {
        var jwt = _authOptions.Jwt;
        if (string.IsNullOrWhiteSpace(jwt.Key))
        {
            throw new InvalidOperationException("JWT authentication key is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddDays(jwt.ExpiryDays);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}