namespace Jam.Models;

public class PlayingSession
{
    public int PlayingSessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; } // Total possible score for the session, makes % calculation easier
    public int CurrentLevel { get; set; }
    public int? CurrentSceneId { get; set; }
    public Scene? CurrentScene { get; set; } // Navigation property 
    public int StoryId { get; set; }
    public Story Story { get; set; } = null!; // Navigation property
    public int? UserId { get; set; } // Foreign key to User, nullable to allow User deletion without deleting playingsessions
    public User? User { get; set; } = null!; // Navigation property 
}
