using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WhatsForDinner.Api.Models.Dtos;
using WhatsForDinner.Api.Services;

namespace WhatsForDinner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string SessionCookieName = "wfd_session";

    private readonly IAuthService _authService;
    private readonly AuthOptions _authOptions;

    public AuthController(IAuthService authService, IOptions<AuthOptions> authOptions)
    {
        _authService = authService;
        _authOptions = authOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AuthUserDto>> SignInWithGoogle([FromBody] GoogleSignInRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Credential))
        {
            return BadRequest(new ErrorResponse("Google credential is required."));
        }

        try
        {
            var user = await _authService.SignInWithGoogleAsync(request.Credential, HttpContext.RequestAborted);
            var token = _authService.CreateSessionToken(user);
            SetSessionCookie(token);

            return Ok(ToDto(user));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception)
        {
            return Unauthorized(new ErrorResponse("Invalid or expired Google credential."));
        }
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthUserDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new ErrorResponse("Invalid session."));
        }

        var user = await _authService.GetUserByIdAsync(userId.Value, HttpContext.RequestAborted);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse("Session user not found."));
        }

        // Sliding expiration: reset cookie on every authenticated identity check.
        var token = _authService.CreateSessionToken(user);
        SetSessionCookie(token);

        return Ok(ToDto(user));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(SessionCookieName, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = HttpContext.Request.IsHttps,
            Path = "/"
        });

        return Ok();
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : null;
    }

    private void SetSessionCookie(string token)
    {
        var expiry = DateTimeOffset.UtcNow.AddDays(_authOptions.Jwt.ExpiryDays);
        Response.Cookies.Append(SessionCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = expiry,
            IsEssential = true,
            Path = "/"
        });
    }

    private static AuthUserDto ToDto(Models.User user)
        => new(user.Id, user.Email, user.DisplayName, user.PictureUrl);
}