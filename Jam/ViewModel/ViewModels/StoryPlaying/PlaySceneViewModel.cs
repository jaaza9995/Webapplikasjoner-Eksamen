using Jam.Models;
using Jam.Models.Enums;

namespace Jam.ViewModels;

public class PlaySceneViewModel
{
    public int SessionId { get; set; }
    public int SceneId { get; set; }
    public string SceneText { get; set; } = string.Empty;
    public SceneType SceneType { get; set; }


    // For Question Scenes
    public Question? Question { get; set; }
    public IEnumerable<AnswerOption>? AnswerOptions { get; set; }
    public int? SelectedAnswerId { get; set; } // user’s selected answer in the form 


    // Playing stats
    public int CurrentScore { get; set; }
    public int MaxScore { get; set; }
    public int CurrentLevel { get; set; }
}