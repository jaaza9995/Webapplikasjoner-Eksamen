
public class AnswerFeedbackViewModel
{
    public string SceneText { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public int StoryId { get; set; }
    public int? NextSceneId { get; set; }
    public int NewScore { get; set; }
    public int NewLevel { get; set; }
}