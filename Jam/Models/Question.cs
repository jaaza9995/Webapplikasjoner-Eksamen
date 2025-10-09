namespace Jam.Models;

public class Question
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int SceneId { get; set; }
    public Scene Scene { get; set; } = null!; // Navigation property
    public List<AnswerOption> AnswerOptions { get; set; } = new(); // Navigation property
}