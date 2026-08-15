using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Application.Authentication.DTOs;

public class SaveAddressRequest
{
    [Required]
    [MaxLength(250)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? AddressLine2 { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }
}