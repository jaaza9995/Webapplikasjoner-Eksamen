using Jam.DAL.AnswerOptionDAL;
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

    // BrowseController
    [HttpGet]
    public async Task<IActionResult> ShowCode(int storyId)
    {
        var story = await _db.Stories.FindAsync(storyId);
        if (story == null || story.Accessible != Accessibility.Private) return NotFound();
        ViewBag.Code = story.GameCode;
        ViewBag.Title = story.Title;
        return View(); // enkelt view som skriver ut ViewBag.Code
    }




}