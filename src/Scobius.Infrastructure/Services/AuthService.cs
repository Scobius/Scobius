using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Scobius.Core.DTOs.Auth;
using Scobius.Core.Entities;
using Scobius.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Scobius.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(UserManager<AppUser> userManager, AppDbContext db, IConfiguration config)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new Exception("Invalid credentials");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new Exception("Invalid credentials");

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken)
                ?? throw new Exception("Invalid refresh token");

        if (token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            throw new Exception("Refresh token expired or revoked");

        token.IsRevoked = true; // rotate — old token invalidated
        await _db.SaveChangesAsync();

        return await GenerateTokensAsync(token.User);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token is null) return;
        token.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    // ──────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────

    private async Task<AuthResponse> GenerateTokensAsync(AppUser user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken,
            AccessTokenExpiry = accessToken.Expiry
        };
    }

    private (string Token, DateTime Expiry) GenerateAccessToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var expiry = DateTime.UtcNow.AddMinutes(
            _config.GetValue<int>("Jwt:AccessTokenExpiryMinutes"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("displayName", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        var token = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _config.GetValue<int>("Jwt:RefreshTokenExpiryDays"))
        };

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();

        return token.Token;
    }
}
