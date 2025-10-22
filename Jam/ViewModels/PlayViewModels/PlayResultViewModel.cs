namespace Jam.ViewModels
{
    public class PlayResultViewModel
    {
        public int StoryId { get; set; }
        public string Title { get; set; } = "";

        // Endings
        public string LowEnding { get; set; } = "";
        public string MediumEnding { get; set; } = "";
        public string HighEnding { get; set; } = "";

        public int Score { get; set; }
        public int Highscore { get; set; }

        // threshold for High vs Medium ending -> usikker om jeg trenger å ha med her
        public int HighScoreThreshold { get; set; }
    }
}
