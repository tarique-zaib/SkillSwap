using System.ComponentModel.DataAnnotations;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Authentication.DTOs;

public class CreateSkillListingRequest
{
    [Required]
    public SkillListingType Type { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Range(0.1, 50)]
    public double RadiusKm { get; set; } = 2;
}