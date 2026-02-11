using System.ComponentModel.DataAnnotations;
using Selu383.SP26.Api.Domain.Identity;

namespace Selu383.SP26.Api.Features.Locations;

public class Location
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TableCount { get; set; }

    public int? ManagerId { get; set; }      // NEW (FK)
    public User? Manager { get; set; }       // NEW (nav)
}
