using System.ComponentModel.DataAnnotations;

namespace Jam.ViewModels;

public class CreateQuestionSceneViewModel
{
    public int StoryId { get; set; }

    public int? PreviousSceneId { get; set; } // to chain scenes together

    // [Required]
    [Display(Name = "Story Text")]
    public string StoryText { get; set; } = string.Empty;

    // [Required]
    [Display(Name = "Question")]
    public string QuestionText { get; set; } = string.Empty;

    public List<AnswerOptionInput> Answers { get; set; } = new()
    {
        new(), new(), new(), new() // ensure 4 answer fields of AnswerOptionInput
    };

    public bool AddAnother { get; set; } // True if user clicks "Next question"
}

public class AnswerOptionInput
{
    // [Required]
    [Display(Name = "Answer Option")]
    public string AnswerText { get; set; } = string.Empty;

    [Display(Name = "Is Correct")]
    public bool IsCorrect { get; set; }

    // [Required]
    [Display(Name = "Follow-up Text")]
    public string ContextText { get; set; } = string.Empty;
}