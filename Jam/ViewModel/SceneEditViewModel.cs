using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Jam.Models.Enums; 
using Jam.Models;

namespace Jam.ViewModels;

public class SceneEditViewModel
{
    public int SceneId { get; set; }
    public int StoryId { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    public SceneType SceneType { get; set; }   // Intro / Question / Ending
    public List<SelectListItem> SceneTypeOptions { get; set; } = new();

    [Required]
    public DifficultyLevel Level { get; set; } // Easy / Medium / Hard
    public List<SelectListItem> LevelOptions { get; set; } = new();

    public int? PreviousSceneId { get; set; }
    public int? NextSceneId { get; set; }

    // Egne lister til select-feltene i viewet
    public List<SelectListItem> PreviousSceneOptions { get; set; } = new();
    public List<SelectListItem> NextSceneOptions { get; set; } = new();
    
}
