namespace Jam.Models;

public class GameSession
{
    public int GameSessionId { get; set; }
    //public DateTime StartTime { get; set; }
    //public DateTime? EndTime { get; set; }
    public int Score { get; set; }

    public int CurrentSceneId { get; set; }

    public int StoryId { get; set; }
    public Story? Story { get; set; } //navigation property 

    public int UserId { get; set; }

    public User? User { get; set; } //navigational property
    
}
