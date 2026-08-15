namespace SkillSwap.Domain.Entities;

public class Vouch
{
    public Guid Id { get; set; }

    public Guid FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public Guid FavorId { get; set; }

    public string SkillCategory { get; set; } = string.Empty;

    public string? StoryText { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public User FromUser { get; set; } = null!;

    public User ToUser { get; set; } = null!;

    public Favor Favor { get; set; } = null!;
}