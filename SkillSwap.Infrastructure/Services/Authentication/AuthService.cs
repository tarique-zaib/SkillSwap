using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkillSwap.Application.Authentication.DTOs;
using SkillSwap.Application.Interfaces;
using SkillSwap.Domain.Entities;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly SkillSwapDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        SkillSwapDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        bool emailExists = await _dbContext.Users
            .AnyAsync(
                x => x.Email == email,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),

            FirstName = request.FirstName.Trim(),

            LastName = request.LastName.Trim(),

            Email = email,

            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim(),

            PasswordHash = _passwordHasher.Hash(request.Password),

            IsActive = true,

            IsAddressVerified = false,

            CreatedAtUtc = DateTime.UtcNow,

            LastActiveAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);

        var address = new Address
        {
            Id = Guid.NewGuid(),

            UserId = user.Id,

            AddressLine1 = request.AddressLine1 ?? string.Empty,
            City = request.City ?? string.Empty,
            State = request.State ?? string.Empty,
            PostalCode = request.PostalCode ?? string.Empty,

            Latitude = request.Latitude,

            Longitude = request.Longitude,

            IsVerified = false,

            VerifiedAtUtc = DateTime.UtcNow
        };

        _dbContext.Addresses.Add(address);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterResponse
        {
            UserId = user.Id,

            FirstName = user.FirstName,

            LastName = user.LastName,

            Email = user.Email,

            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                x => x.Email == email,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "This account has been deactivated.");
        }

        bool passwordValid = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        user.LastActiveAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        string accessToken =
            _jwtTokenService.GenerateToken(user);
        Console.WriteLine(
    $"JWT generated at: {DateTime.UtcNow:O}");

        int expirationMinutes =
            _configuration.GetValue<int>(
                "Jwt:ExpirationMinutes");

        DateTime expiresAtUtc =
            DateTime.UtcNow.AddMinutes(expirationMinutes);

        return new LoginResponse
        {
            AccessToken = accessToken,

            ExpiresAtUtc = expiresAtUtc,

            UserId = user.Id,

            FirstName = user.FirstName,

            LastName = user.LastName,

            Email = user.Email
        };
    }

    public async Task<CurrentUserProfileDto?> GetCurrentUserAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Address)
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new CurrentUserProfileDto
        {
            UserId = user.Id,

            FirstName = user.FirstName,

            LastName = user.LastName,

            Email = user.Email,

            PhoneNumber = user.PhoneNumber,

            CreatedAtUtc = user.CreatedAtUtc,

            IsAddressVerified = user.IsAddressVerified,

            Address = user.Address == null
                ? null
                : new AddressProfileDto
                {
                    AddressLine1 = user.Address.AddressLine1,

                    AddressLine2 = user.Address.AddressLine2,

                    City = user.Address.City,

                    State = user.Address.State,

                    PostalCode = user.Address.PostalCode,

                    IsVerified = user.Address.IsVerified
                }
        };
    }
}