namespace Jam.ViewModels;

public class StoryDetailsViewModel
{
    public int StoryId { get; set; }
    public string Title { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";

    // For visning i detaljsiden
    public int SceneCount { get; set; }
    public int QuestionCount { get; set; }

    // Liste over scener (valgfritt)
    public IEnumerable<SceneSummaryVM> Scenes { get; set; } = new List<SceneSummaryVM>();
}

public class SceneSummaryVM
{
    public int SceneId { get; set; }
    public string Title { get; set; } = "";
    public string SceneType { get; set; } = "";
}
