using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jam.DAL;              // din DbContext
using Jam.Models;
using Jam.Models.Enums;
using Jam.ViewModels;
using System.Linq;

namespace Jam.Controllers
{
    public class BrowseController : Controller
    {
        private readonly StoryRepository _db; // bytt til din DbContext om den heter noe annet

        public BrowseController(StoryRepository db)
        {
            _db = db;
        }

        // GET /Browse?q=...
        // Viser offentlige spill + supportsøk på tittel
        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Stories
                .Include(s => s.Scenes)
                .Where(s => s.IsPublic); // forutsetter et felt IsPublic

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(s => EF.Functions.Like(s.Title, $"%{q}%"));

            var stories = await query
                .OrderBy(s => s.Title)
                .Select(s => new BrowseCardViewModel
                {
                    StoryId = s.StoryId,
                    Title = s.Title,
                    Description = s.Description ?? "",
                    Code = s.Code, // kan brukes til Play via kode om ønskelig
                    QuestionCount = s.Scenes.Count(x => x.SceneType == SceneType.Question),
                    Level = s.Scenes.Any()
                        ? s.Scenes.Select(x => x.Level).OrderBy(x => x).First() // enkel representant
                        : DifficultyLevel.Easy,
                    IntroPreview = s.Scenes
                        .Where(x => x.SceneType == SceneType.Intro)
                        .OrderBy(x => x.SceneId)
                        .Select(x => x.Body)
                        .FirstOrDefault() ?? "", // kort intro-tekst i kortet
                })
                .ToListAsync();

            var vm = new BrowseIndexViewModel
            {
                Query = q ?? "",
                Cards = stories,
                PrivateCode = new EnterCodeViewModel()
            };

            return View(vm);
        }

        // POST /Browse/Private (rosa boksen til høyre)
        [HttpPost]
        public IActionResult Private(EnterCodeViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Code))
            {
                TempData["PrivateError"] = "Skriv inn en gyldig kode.";
                return RedirectToAction(nameof(Index));
            }

            // send brukeren til PlayController som starter via kode
            return RedirectToAction("StartByCode", "Play", new { code = model.Code });
        }
    }
}
