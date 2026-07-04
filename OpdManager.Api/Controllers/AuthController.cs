using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpdManager.Api.Models;
using OpdManager.Infrastructure.Data;
using OpdManager.Infrastructure.Identity;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        AppDbContext db)
    {
        _userManager = userManager;
        _config = config;
        _db = db;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        var expirationMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Staff"
            }
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked);

        if (token is null || token.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        // Revoke old token
        token.IsRevoked = true;
        _db.RefreshTokens.Update(token);

        var user = token.User;
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

        var expirationMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Staff"
            }
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserInfo
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "Staff"
        });
    }

    private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expirationMinutes = int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        var refreshTokenDays = int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7");
        var token = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return token.Token;
    }
}
