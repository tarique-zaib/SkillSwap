namespace SkillSwap.Application.DTOs;

public class ReciprocitySummaryDto
{
    public int GivenCount { get; set; }

    public int ReceivedCount { get; set; }

    public int Balance { get; set; }

    public int CompletedFavorCount { get; set; }
}