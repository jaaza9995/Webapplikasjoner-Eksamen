using Microsoft.AspNetCore.Mvc;
using Jam.DAL.StoryDAL;
using Jam.ViewModels;

namespace Jam.Controllers
{
    public class BrowseController : Controller
    {
        private readonly IStoryRepository _stories;

        public BrowseController(IStoryRepository stories)
        {
            _stories = stories;
        }
        // GET /Browse
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var allStories = await _stories.GetAllPublicStories();

            if (!string.IsNullOrWhiteSpace(search))
                allStories = allStories
                    .Where(s => s.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

            // Bruk eksisterende PlayNewPublicGameViewModel
            var vm = new PlayNewPublicGameViewModel
            {
                SearchQuery = search,
                Stories = allStories.Select(s => new StoryListViewModel
                {
                    Id = s.StoryId,
                    Title = s.Title,
                    Description = s.Description,
                    QuestionCount = s.Scenes.Count(x => x.SceneType == Models.Enums.SceneType.Question)
                })
            };

            return View(vm);
        }

        // POST /Browse/Private
        [HttpPost]
        public async Task<IActionResult> EnterCode(EnterCodeViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            var story = await _stories.GetPrivateStoryByCode(model.Code);
            if (story == null)
            {
                TempData["Error"] = "Ugyldig kode.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("StartByCode", "Play", new { code = model.Code });
        }
    }
}
