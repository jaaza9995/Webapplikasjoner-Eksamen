using Jam.Models.Enums;

public class StartStoryViewModel
{
    public int StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Accessibility Accessibility { get; set; }
    public string? Code { get; set; } // user input if story is private
    public string? ErrorMessage { get; set; } // if user types in the wrong code
}