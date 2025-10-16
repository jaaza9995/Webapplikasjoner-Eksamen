using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jam.ViewModels;

public class EditStoryViewModel
{
    public int StoryId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string DifficultyLevel { get; set; } = "";

    // Fills the dropdown menu
    public List<SelectListItem> DifficultyLevels { get; set; } = new();
    public List<SceneEditViewModel> Scenes { get; set; } = new();
}

public class SceneEditViewModel
{
    public int SceneId { get; set; }
    public string SceneText { get; set; } = "";
    public QuestionEditViewModel Question { get; set; } = new();
}

public class QuestionEditViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = "";
    public List<string> Answers { get; set; } = new();
}
