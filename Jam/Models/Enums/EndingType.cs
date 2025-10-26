using System.ComponentModel.DataAnnotations;

namespace Jam.Models.Enums;

public enum EndingType
{
    [Required]Good,
    [Required]Neutral,
    [Required]Bad
}