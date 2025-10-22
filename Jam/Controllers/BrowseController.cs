/*using Jam.DAL.AnswerOptionDAL;
using Jam.DAL;
using Jam.DAL.StoryDAL;
using Microsoft.AspNetCore.Mvc;
using Jam.ViewModels;
using Jam.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Jam.Controllers;


public class BrowoseController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly IAnswerOptionRepository _answers;
    private readonly StoryDbContext _db;

    public BrowoseController(
        IAnswerOptionRepository answerOptionRepository,
        IStoryRepository storiesRepository,
        StoryDbContext db)
    {
        _answers = answerOptionRepository;
        _stories = storiesRepository;
        _db = db;
    }


    [HttpGet]
    public async Task<IActionResult> Browse(string? search)
    {
        var stories = await _db.Stories
            .Where(s => s.Accessible == Accessibility.Public &&
                        (string.IsNullOrEmpty(search) ||
                         s.Title.Contains(search)))
            .Include(s => s.QuestionScenes)
            .ToListAsync();

        var model = stories.Select(s => new GameCardViewModel
        {
            StoryId = s.StoryId,
            Title = s.Title,
            Description = s.Description,
            DifficultyLevel = s.DifficultyLevel.ToString(),
            NumberOfQuestions = s.QuestionScenes?.Count ?? 0
        }).ToList();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AccessPrivate(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return RedirectToAction(nameof(Browse));

        var story = await _db.Stories.FirstOrDefaultAsync(s => s.Code == code);
        if (story == null)
        {
            TempData["Error"] = "No game found with that code.";
            return RedirectToAction(nameof(Browse));
        }

        // Gå videre til selve spillingen (f.eks. PlayController.Start)
        return RedirectToAction("Start", "Play", new { storyId = story.StoryId });
    }


}*/