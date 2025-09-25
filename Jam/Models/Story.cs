namespace Jam.Models;

public class Story
{
    public int StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DifficultyLevel { get; set; }
    public string? Code { get; set; }
    public int Played { get; set; }
    public int Finished { get; set; }
    public int Died { get; set; }
    public int Dnf { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; } // navigational property
    
    // public string? ImageUrl { get; set; }

}