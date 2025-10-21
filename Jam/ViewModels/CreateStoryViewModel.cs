using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Jam.ViewModels
{
    public class CreateStoryViewModel
    {
        // Story details
        public int StoryId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        public string Accessibility { get; set; } = "";

        // Dropdown lists
        public List<SelectListItem> DifficultyOptions { get; set; } = new()
        {
            new SelectListItem("Easy", "Easy"),
            new SelectListItem("Medium", "Medium"),
            new SelectListItem("Hard", "Hard")
        };

        public List<SelectListItem> AccessibilityOptions { get; set; } = new()
        {
            new SelectListItem("Public", "Public"),
            new SelectListItem("Private", "Private")
        };

        // Questions and answers
        public List<CreateQuestionViewModel> Questions { get; set; } = new();

        // Endings
        public string HighEnding { get; set; } = "";
        public string MediumEnding { get; set; } = "";
        public string LowEnding { get; set; } = "";

        // Current step in the process
        public int Step { get; set; } = 1;
    }

    public class CreateQuestionViewModel
    {
        public string QuestionText { get; set; } = "";
        public List<string> Answers { get; set; } = new() { "", "", "", "" };
        public List<string> Feedbacks { get; set; } = new() { "", "", "", "" };
    }
}
