using Jam.Models.Enums;
using Jam.Models;
using System.ComponentModel.DataAnnotations;

public class StoryEditOverviewViewModel
{
    public int StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;


    [Required(ErrorMessage = "Please select a difficulty level")]
    public DifficultyLevel? DifficultyLevel { get; set; }

    [Required(ErrorMessage = "Please select accessibility")]
    public Accessibility? Accessibility { get; set; }
    public IEnumerable<Scene> Scenes { get; set; } = new List<Scene>();
}