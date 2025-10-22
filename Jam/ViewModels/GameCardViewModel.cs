using Jam.Models.Enums;

namespace Jam.ViewModels;

public class GameCardViewModel
{
    public string StoryId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int NumberOfQuestions { get; set; }
    public string DifficultyOptions { get; set; } = "";
    public Accessibility Accessible { get; set; }
        // New property returns if game is public
    public bool IsPublic => Accessible == Accessibility.Public;
    public string GameCode { get; set; } = "";
    public bool ShowEditButton { get; set; } = false;
    public int? Highscore { get; set; } // optional, for recently played
    // sets default GameCardType to BrowseMode since it has the least features/info
    public GameCardType CardType { get; set; } = GameCardType.BrowseMode;
}