namespace SkillSwap.Application.DTOs;

public class TrustSummaryDto
{
    public string Tier { get; set; } = string.Empty;

    public int CompletedFavorCount { get; set; }

    public int VouchCount { get; set; }

    public int GivenFavorCount { get; set; }

    public int ReceivedFavorCount { get; set; }
}