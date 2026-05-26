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
using Microsoft.AspNetCore.WebUtilities;

namespace Scobius.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public AuthService(UserManager<AppUser> userManager, AppDbContext db, IConfiguration config, IEmailService email)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
        _emailService = email;
    }


    public async Task<string> RegisterAsync(RegisterRequest request)
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

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationLink = $"{_config["App:BaseUrl"]}/api/auth/verify-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailVerificationAsync(user.Email!, user.DisplayName, confirmationLink);

        // Don't return tokens yet — user must verify email first
        return "Registration successful. Please check your email to verify your account.";
    }

    public async Task<AuthResponse> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception("User not found");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            throw new Exception("Invalid or expired verification token");

        // Email confirmed — now issue tokens
        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.EmailOrUsername)
            ?? await _userManager.FindByNameAsync(request.EmailOrUsername)
            ?? throw new Exception("Invalid credentials");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new Exception("Invalid credentials");

        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new Exception("Please verify your email before logging in");

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
