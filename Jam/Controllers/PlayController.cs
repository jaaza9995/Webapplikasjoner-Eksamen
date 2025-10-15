using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jam.DAL;
using Jam.Models;
using Jam.Models.Enums;
using Jam.ViewModels;

namespace Jam.Controllers
{
    // Styrer flyten: start -> scene -> svar/neste -> slutt
    public class PlayController : Controller
    {
        private readonly StoryDbContext _db;
        public PlayController(StoryDbContext db) { _db = db; }

        [HttpPost]
        public async Task<IActionResult> StartByCode(string code, int sessionId = 1)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest("Kode mangler.");

            var story = await _db.Stories.FirstOrDefaultAsync(s => s.Code == code);
            if (story == null) return BadRequest("Ugyldig kode.");

            // 1) Prøv å finne introduksjonsscene
            var intro = await _db.Scenes
                .Where(s => s.StoryId == story.StoryId && s.SceneType == SceneType.Introduction)
                .OrderBy(s => s.SceneId)
                .FirstOrDefaultAsync();

            // 2) Hvis ingen intro: ta laveste SceneId i historien
            var first = intro ?? await _db.Scenes
                .Where(s => s.StoryId == story.StoryId)
                .OrderBy(s => s.SceneId)
                .FirstAsync();

            return RedirectToAction(nameof(Scene), new { sceneId = first.SceneId, sessionId });
        }

   [HttpGet]
        public async Task<IActionResult> Scene(int sceneId, int sessionId)
        {
            var s = await _db.Scenes
                .Include(x => x.Question!)
                .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(x => x.SceneId == sceneId);

            if (s == null) return NotFound("Scene ikke funnet.");

            var isQuestion = s.Question != null && (s.Question.AnswerOptions?.Any() ?? false);

            var vm = new PlaySceneViewModel
            {
                SessionId  = sessionId,
                SceneId    = s.SceneId,
                SceneText  = s.SceneText ?? string.Empty,
                SceneType  = s.SceneType.ToString(),
                IsQuestion = isQuestion,
                NextSceneId = s.NextSceneId,
                Answers = (s.Question?.AnswerOptions ?? new List<AnswerOption>())
                    .OrderBy(a => a.AnswerOptionId) // ← endre til riktig PK-navn hvis annet
                    .Select(a => new AnswerOptionViewModel
                    {
                        AnswerId = a.AnswerOptionId,   // ← endre til riktig felt (f.eks. Id/AnswerId)
                       Text = (string?)
                       (
                            a.GetType().GetProperty("Text")?.GetValue(a)
                            ?? a.GetType().GetProperty("OptionText")?.GetValue(a)
                            ?? a.GetType().GetProperty("AnswerText")?.GetValue(a)
                            ?? a.GetType().GetProperty("Label")?.GetValue(a)
                            ?? a.GetType().GetProperty("Content")?.GetValue(a)
                            ?? a.GetType().GetProperty("Body")?.GetValue(a)
                        ) ?? string.Empty
                    })
            };

            return View("Scene", vm);
        }


        // For scener uten svar (ikke-spørsmål)
        [HttpPost]
        public async Task<IActionResult> Next(int sessionId, int sceneId)
        {
            var s = await _db.Scenes.FindAsync(sceneId);
            if (s == null) return NotFound("Scene ikke funnet.");

            if (s.NextSceneId == null)
                return RedirectToAction(nameof(End), new { sessionId });

            return RedirectToAction(nameof(Scene), new { sceneId = s.NextSceneId.Value, sessionId });
        }

        // For spørsmålsscener – bruker valgt svar
       [HttpPost]
            public async Task<IActionResult> Answer(int sessionId, int answerId)
            {
                var a = await _db.Set<AnswerOption>().FindAsync(answerId); // AnswerOption, ikke Answer
                if (a == null) return NotFound("Svar ikke funnet.");

                return a.NextSceneId == null
                    ? RedirectToAction(nameof(End), new { sessionId })
                    : RedirectToAction(nameof(Scene), new { sceneId = a.NextSceneId.Value, sessionId });
            }
        [HttpGet]
        public IActionResult End(int sessionId) => View(model: sessionId);
    }
}
