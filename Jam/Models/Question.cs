namespace Jam.Models;

public class Question
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public string WrongAnswer1 { get; set; } = string.Empty;
    public string WrongAnswer2 { get; set; } = string.Empty;
    public string WrongAnswer3 { get; set; } = string.Empty;

    public string? HelpingText { get; set; }

    public int SceneId { get; set; }

    public Scene? Scene { get; set;} // navigational property
}