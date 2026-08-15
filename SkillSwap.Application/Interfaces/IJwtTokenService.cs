using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}