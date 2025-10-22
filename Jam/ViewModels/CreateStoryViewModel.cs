using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Jam.Models.Enums;

namespace Jam.ViewModels
{
    public class CreateStoryViewModel
    {
        // Game details
        public int StoryId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Intro { get; set; } = "";
        public List<SelectListItem> DifficultyOptions { get; set; } = new();
        public DifficultyLevel DifficultyLevel { get; set; }
        public List<SelectListItem> AccessibilityOptions { get; set; } = new();
        public Accessibility Accessibility { get; set; }
       
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
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = "";
        public List<string> Answers { get; set; } = new() { "", "", "", "" };
        public List<string> Feedbacks { get; set; } = new() { "", "", "", "" };
        public List<bool> IsCorrect { get; set; } = new() { false, false, false, false };
    }
}