using Jam.Models.Enums;

namespace Jam.Models;

public class Scene
{
    public int SceneId { get; set; }
    public SceneType SceneType { get; set; } // Introduction || Question || Ending
    public string SceneText { get; set; } = string.Empty;
    public int? NextSceneId { get; set; }
    public Scene? NextScene { get; set; } // Navigation property
    public int StoryId { get; set; }
    public Story Story { get; set; } = null!; // Navigation property
    public Question? Question { get; set; } // Navigation property, only if SceneType == Question
}

/*
    Potential circular nav issue: NextScene pointing back to Scene
    Have to be careful when querying with EF .Include(), as it may 
    try to auto-load recursively. Might want to configure 
    .WithMany().HasForeignKey(...) in OnModelCreating
*/