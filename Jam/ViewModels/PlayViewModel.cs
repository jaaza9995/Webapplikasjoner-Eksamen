using System.Collections.Generic;

namespace Jam.ViewModels
{
    public class PlayViewModel
    {
        public string Title { get; set; } = "";
        public int SceneId { get; set; }
        public string Question { get; set; } = "";
        public List<string> Answers { get; set; } = new();
        public List<string> Feedbacks { get; set; } = new();
        public int Lives { get; set; } = 3; // antall liv
    }
}
