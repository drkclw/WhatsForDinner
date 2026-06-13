using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WhatsForDinner.Api.Data;
using WhatsForDinner.Api.Models;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Tests.Integration;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                               d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_Auth_{Guid.NewGuid()}");
                });

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                services.AddScoped<IAuthService, FakeAuthService>();

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task SignInWithGoogle_ReturnsOk_AndSetsCookie()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/google", new GoogleSignInRequest("valid-credential"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().Contain(h => h.Key == "Set-Cookie");
        var body = await response.Content.ReadFromJsonAsync<AuthUserDto>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("user1@example.com");
    }

    [Fact]
    public async Task SignInWithGoogle_ReturnsUnauthorized_ForInvalidCredential()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/google", new GoogleSignInRequest("bad-credential"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsAuthenticatedUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "1");

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthUserDto>();
        body.Should().NotBeNull();
        body!.DisplayName.Should().Be("User One");
    }

    private class FakeAuthService : IAuthService
    {
        public Task<User> SignInWithGoogleAsync(string credential, CancellationToken cancellationToken = default)
        {
            if (credential != "valid-credential")
            {
                throw new Exception("invalid credential");
            }

            return Task.FromResult(new User
            {
                Id = 1,
                GoogleId = "google-user-1",
                Email = "user1@example.com",
                DisplayName = "User One",
                PictureUrl = "https://example.com/avatar.jpg"
            });
        }

        public Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId != 1)
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(new User
            {
                Id = 1,
                GoogleId = "google-user-1",
                Email = "user1@example.com",
                DisplayName = "User One",
                PictureUrl = "https://example.com/avatar.jpg"
            });
        }

        public string CreateSessionToken(User user)
        {
            return "fake-session-token";
        }
    }
}