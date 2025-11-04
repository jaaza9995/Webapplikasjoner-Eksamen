using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Jam.ViewModels;
using Jam.Models;    
using Jam.DAL.StoryDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.Models.Enums;
using Jam.DAL;
using Jam.DAL.SceneDAL;

namespace Jam.Controllers;

public class EditGameController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly ISceneRepository _scenes;
    private readonly IAnswerOptionRepository _answers;

    public EditGameController(
        IAnswerOptionRepository answerOptionRepository,
        IStoryRepository storiesRepository,
        ISceneRepository sceneRepository
        )
    {
        _answers = answerOptionRepository;
        _stories = storiesRepository;
        _scenes = sceneRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var story = await _stories.GetStoryById(id);
        if (story == null) return NotFound();

        var model = new EditStoryViewModel
        {
            StoryId = story.StoryId,
            Title = story.Title,
            Description = story.Description,
            DifficultyLevel = story.DifficultyLevel,
            Accessibility = story.Accessible,
            GameCode = story.GameCode,
            Step = 1,
            IsEditMode = true,
            DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList(),
            AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList()
        };

        return View("Edit", model);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditStoryViewModel model, string action)
    {
        if (action == "Back")
        {
            model.Step = Math.Max(1, model.Step - 1);
            return View("Edit", model);
        }

        if (action == "Next")
        {
            model.Step++;
            return View("Edit", model);
        }

        if (action == "Finish")
        {
            if (!ModelState.IsValid)
            {
                RepopulateDropdowns(model);
                return View("Edit", model);
            }

            try
            {
                var story = await _stories.GetStoryById(model.StoryId);
                await _stories.UpdateStory(story);
                if (story == null) return NotFound();

                story.Title = model.Title ?? "";
                story.Description = model.Description ?? "";
                story.DifficultyLevel = model.DifficultyLevel;

                if (story.Accessible != model.Accessibility)
                {
                    story.Accessible = model.Accessibility;
                    if (story.Accessible == Accessibility.Private && string.IsNullOrEmpty(story.GameCode))
                        story.GameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
                    if (story.Accessible == Accessibility.Public)
                        story.GameCode = null;
                }

                await _stories.UpdateStory(story);
                return RedirectToAction("Details", "Story", new { id = story.StoryId });
            }
            catch (Exception)
            {
                return Problem("Uventet feil under lagring.");
            }
        }

        ModelState.AddModelError(string.Empty, "Ukjent handling.");
        return View("Edit", model);
    }
        private void RepopulateDropdowns(EditStoryViewModel model)
    {
        model.DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
            .Cast<DifficultyLevel>()
            .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
            .ToList();

        model.AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
            .Cast<Accessibility>()
            .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
            .ToList();
    }


}