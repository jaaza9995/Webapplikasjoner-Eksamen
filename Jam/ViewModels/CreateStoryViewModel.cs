using Microsoft.AspNetCore.Mvc.Rendering;
using Jam.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Jam.ViewModels
{
    public class CreateStoryViewModel
    {
        // Game details
        public int StoryId { get; set; }
        [Required] public string? Title { get; set; } 
        [Required] public string? Description { get; set; } 
        [Required] public string? Intro { get; set; } 

        public List<SelectListItem> DifficultyOptions { get; set; } = default!;
        
        [Required(ErrorMessage = "Please select a difficulty level.")]
        public DifficultyLevel DifficultyLevel { get; set; }
        public List<SelectListItem> AccessibilityOptions { get; set; } = new();
        [Required(ErrorMessage = "Please select a privacy option.")]
        public Accessibility Accessibility { get; set; } 

        // Questions and answers
        public List<CreateQuestionViewModel> Questions { get; set; } = new();

        // Endings
        public string HighEnding { get; set; } = "";
        public string MediumEnding { get; set; } = "";
        public string LowEnding { get; set; } = "";

        // Current step in the process
        public int Step { get; set; } = 1;
        public string? GameCode { get; set; } 
    }

    public class CreateQuestionViewModel
    {
        
        public int QuestionId { get; set; }

        [Required] public string QuestionText { get; set; } = "";

        [Required] public List<string> Answers { get; set; } = new() { "", "", "", "" };

        [Required] public List<string> Feedbacks { get; set; } = new() { "", "", "", "" };

        [Required] public List<bool> IsCorrect { get; set; } = new() { false, false, false, false };
    }
}