using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Jam.Models.Enums;
using Jam.ViewModels;
using Jam.DAL;

public class SceneController : Controller
{
    private readonly StoryDbContext _db;
    public SceneController(StoryDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Create(int storyId)
    {
        var vm = new SceneEditViewModel
        {
            StoryId = storyId,
            SceneTypeOptions = Enum.GetValues(typeof(SceneType))
                .Cast<SceneType>()
                .Select(t => new SelectListItem
                {
                    Text = t.ToString(),
                    Value = t.ToString()
                })
                .ToList(),

            LevelOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(l => new SelectListItem
                {
                    Text = l.ToString(),
                    Value = l.ToString()
                })
                .ToList(),

            PreviousSceneOptions = _db.Scenes
                .Where(s => s.StoryId == storyId)
                .Select(s => new SelectListItem
                {
                    Text = s.SceneText,
                    Value = s.SceneId.ToString()
                })
                .ToList(),

            NextSceneOptions = _db.Scenes
                .Where(s => s.StoryId == storyId)
                .Select(s => new SelectListItem
                {
                    Text = s.SceneText,
                    Value = s.SceneId.ToString()
                })
                .ToList()
        };

        return View(vm);
    }
}
