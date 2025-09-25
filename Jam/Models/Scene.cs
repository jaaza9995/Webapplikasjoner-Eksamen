namespace Jam.Models;

public class Scene
{
    public int SceneId { get; set; }
    public string SceneType { get; set; } = string.Empty;
    public string Scenetext { get; set; } = string.Empty;
    public int? NextSceneId { get; set; }

    public int StoryId { get; set; }
    public Story? Story { get; set; }


    // public string? ImageUrl { get; set; }
}