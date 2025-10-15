using Microsoft.AspNetCore.Mvc.Rendering;
using Jam.Models.Enums; 

namespace Jam.ViewModels;

public class PlayStepViewModel
{
    public Guid SessionId { get; set; }
    public string StoryTitle { get; set; } = default!;
    public int Score { get; set; }
    public int Hearts { get; set; }
    public DifficultyLevel Level { get; set; }

}
