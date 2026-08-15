using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Interfaces;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Services;
using SkillSwap.Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<SkillSwapDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "SkillSwapDatabase")));

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ISkillListingService, SkillListingService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IFavorService, FavorService>();
builder.Services.AddScoped<IVouchService, VouchService>();
builder.Services.AddScoped<IReciprocityService, ReciprocityService>();
builder.Services.AddScoped<ITrustService, TrustService>();
builder.Services.AddScoped<IChatService, ChatService>();

string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT key is not configured.");

string jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

string jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateIssuer = true,

                ValidIssuer = jwtIssuer,

                ValidateAudience = true,

                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();