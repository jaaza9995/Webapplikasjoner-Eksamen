namespace Jam.Models;

public class AnswerOption
{
    public int AnswerOptionId { get; set; }
    public int? NextSceneId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public string SceneText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    //public string? Feedback { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!; // Navigation property
}