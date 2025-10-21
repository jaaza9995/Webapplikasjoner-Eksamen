namespace Jam.ViewModels;

public class CreateEndingsViewModel
{
    public int StoryId { get; set; }

    // [Required]
    public string GoodEnding { get; set; } = string.Empty;

    // [Required]
    public string NeutralEnding { get; set; } = string.Empty;

    // [Required] 
    public string BadEnding { get; set; } = string.Empty;
}