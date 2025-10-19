using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Jam.Models.Enums;

namespace Jam.ViewModels;

public class CreateStoryAndIntroViewModel
{
    // [Required]
    public string Title { get; set; } = string.Empty;

    // [Required]
    public string Description { get; set; } = string.Empty;

    // [Required]
    [Display(Name = "Difficulty Level")]
    public DifficultyLevel DifficultyLevel { get; set; }

    // [Required]
    [Display(Name = "Accessibility")]
    public Accessibility Accessibility { get; set; }

    // [Required]
    public string IntroText { get; set; } = string.Empty;

    // These two are for the dropdowns for choosing difficulty level and accessibility
    public List<SelectListItem> DifficultyLevelOptions { get; set; } = new();
    public List<SelectListItem> AccessibilityOptions { get; set; } = new();
}