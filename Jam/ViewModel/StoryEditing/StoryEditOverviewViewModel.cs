using Jam.Models.Enums;
using Jam.Models;

public class StoryEditOverviewViewModel
{
    public int StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public Accessibility Accessibility { get; set; }
    public IEnumerable<Scene> Scenes { get; set; } = new List<Scene>();
}