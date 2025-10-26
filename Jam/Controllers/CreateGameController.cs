using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jam.ViewModels;
using Jam.Models;
using Jam.Models.Enums;
using Jam.DAL;

namespace Jam.Controllers
{
    public class CreateGameController : Controller
    {
        private readonly StoryDbContext _db;
        private readonly ILogger<CreateGameController> _logger;
        private const string DraftKey = "CreateGame_Draft";

        public CreateGameController(StoryDbContext db, ILogger<CreateGameController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ---------- Session helpers ----------
        private EditStoryViewModel? LoadDraft()
            => HttpContext.Session.TryGetValue(DraftKey, out var _)
               ? System.Text.Json.JsonSerializer.Deserialize<EditStoryViewModel>(
                    HttpContext.Session.GetString(DraftKey)!)
               : null;

        private void SaveDraft(EditStoryViewModel model)
            => HttpContext.Session.SetString(
                DraftKey,
                System.Text.Json.JsonSerializer.Serialize(model));

        private void ClearDraft() => HttpContext.Session.Remove(DraftKey);

        private static void EnsureLists(EditStoryViewModel m)
        {
            m.Questions ??= new();
            foreach (var q in m.Questions)
            {
                q.Answers   ??= new(); while (q.Answers.Count   < 4) q.Answers.Add("");
                q.Feedbacks ??= new(); while (q.Feedbacks.Count < 4) q.Feedbacks.Add("");
                q.IsCorrect ??= new(); while (q.IsCorrect.Count < 4) q.IsCorrect.Add(false);
            }
        }

        private static void MergePostedIntoDraft(EditStoryViewModel draft, EditStoryViewModel posted)
        {
            // Intro/metadata
            draft.Title           = posted.Title;
            draft.Description     = posted.Description;
            draft.Intro           = posted.Intro;
            draft.DifficultyLevel = posted.DifficultyLevel;
            draft.Accessibility   = posted.Accessibility;
            draft.GameCode        = posted.GameCode;

            // Spørsmål
            draft.Questions = posted.Questions ?? new();
            EnsureLists(draft);

            // Endings
            draft.HighEnding   = posted.HighEnding;
            draft.MediumEnding = posted.MediumEnding;
            draft.LowEnding    = posted.LowEnding;
        }

        private void RepopulateDropdowns(EditStoryViewModel m)
        {
            m.DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList();

            m.AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList();
        }

        // ---------- GET ----------
        [HttpGet]
        public IActionResult Create()
        {
            // Start fresh draft
            var vm = new EditStoryViewModel
            {
                IsEditMode = false,
                Step = 1
            };
            RepopulateDropdowns(vm);
            EnsureLists(vm);
            SaveDraft(vm);
            return View(vm);
        }

        // ---------- POST ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(EditStoryViewModel model, string action)
        {
            var act = (action ?? "").Trim();
            var draft = LoadDraft() ?? new EditStoryViewModel { Step = 1 };
            RepopulateDropdowns(draft);

            // Slå alltid sammen det brukeren nettopp skrev inn i minnet før noe annet
            MergePostedIntoDraft(draft, model);

            // ----- Back (ingen validering, bare lagre og gå tilbake) -----
            if (act.Equals("Back", StringComparison.OrdinalIgnoreCase))
            {
                if (draft.Step <= 1)
                {
                    ClearDraft();
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                draft.Step = Math.Max(1, draft.Step - 1);

                EnsureLists(draft);

                SaveDraft(draft);
                ModelState.Clear();
                RepopulateDropdowns(draft);
                return View("Create", draft);
            }

            // ----- AddQuestion (ingen validering, bli på step 2) -----
            if (act.Equals("AddQuestion", StringComparison.OrdinalIgnoreCase))
            {
                draft.Step = 2;
                EnsureLists(draft);

                draft.Questions.Add(new CreateQuestionViewModel
                {
                    Answers   = new() { "", "", "", "" },
                    Feedbacks = new() { "", "", "", "" },
                    IsCorrect = new() { false, false, false, false }
                });

                SaveDraft(draft);
                ModelState.Clear();
                return View("Create", draft);
            }

            // ----- Next -----
            if (act.Equals("Next", StringComparison.OrdinalIgnoreCase))
            {
                if (draft.Step == 1)
                {
                    // Ingen validering på Next fra intro; gå videre og lagre
                    if (draft.Questions.Count == 0)
                    {
                        draft.Questions.Add(new CreateQuestionViewModel
                        {
                            Answers   = new() { "", "", "", "" },
                            Feedbacks = new() { "", "", "", "" },
                            IsCorrect = new() { false, false, false, false }
                        });
                    }

                    draft.Step = 2;
                    SaveDraft(draft);
                    ModelState.Clear();
                    return View("Create", draft);
                }

                if (draft.Step == 2)
                {
                    // Valider ALLE spørsmål før step 3
                    EnsureLists(draft);
                    bool anyErrors = false;

                    for (int i = 0; i < draft.Questions.Count; i++)
                    {
                        var q = draft.Questions[i];
                        if (string.IsNullOrWhiteSpace(q.QuestionText))
                        { ModelState.AddModelError($"Questions[{i}].QuestionText", "The QuestionText field is required."); anyErrors = true; }

                        for (int j = 0; j < 4; j++)
                        {
                            if (string.IsNullOrWhiteSpace(q.Answers[j]))
                            { ModelState.AddModelError($"Questions[{i}].Answers[{j}]", $"Answer {j + 1} is required."); anyErrors = true; }
                            if (string.IsNullOrWhiteSpace(q.Feedbacks[j]))
                            { ModelState.AddModelError($"Questions[{i}].Feedbacks[{j}]", $"Feedback {j + 1} is required."); anyErrors = true; }
                        }

                        if (q.IsCorrect.Count(x => x) != 1)
                        { ModelState.AddModelError($"Questions[{i}].IsCorrect", "Exactly one answer must be checked as correct."); anyErrors = true; }
                    }

                    if (anyErrors)
                    {
                        // Behold step 2 med feilmeldinger, men data er allerede lagret
                        SaveDraft(draft);
                        return View("Create", draft);
                    }

                    draft.Step = 3;
                    if (draft.Accessibility == Accessibility.Private && string.IsNullOrWhiteSpace(draft.GameCode))
                        draft.GameCode = await GenerateUniqueCodeAsync();

                    SaveDraft(draft);
                    ModelState.Clear();
                    return View("Create", draft);
                }
            }

            // ----- Finish (endelig validering og DB-lagring) -----
            if (act.Equals("Finish", StringComparison.OrdinalIgnoreCase))
            {
                // Valider endings
                if (string.IsNullOrWhiteSpace(draft.HighEnding))
                    ModelState.AddModelError(nameof(draft.HighEnding), "Good ending is required.");
                if (string.IsNullOrWhiteSpace(draft.MediumEnding))
                    ModelState.AddModelError(nameof(draft.MediumEnding), "Neutral ending is required.");
                if (string.IsNullOrWhiteSpace(draft.LowEnding))
                    ModelState.AddModelError(nameof(draft.LowEnding), "Bad ending is required.");
                    

                if (!ModelState.IsValid)
                {
                    draft.Step = 3;
                    SaveDraft(draft);
                    return View("Create", draft);
                }

                if (draft.Accessibility == Accessibility.Private && string.IsNullOrWhiteSpace(draft.GameCode))
                    draft.GameCode = await GenerateUniqueCodeAsync();

                var story = new Story
                {
                    Title          = draft.Title,
                    Description    = draft.Description,
                    DifficultyLevel= draft.DifficultyLevel,
                    Accessible     = draft.Accessibility,
                    GameCode       = draft.Accessibility == Accessibility.Private ? draft.GameCode : null
                };

                _db.Stories.Add(story);
                await _db.SaveChangesAsync();

                _db.IntroScenes.Add(new IntroScene { StoryId = story.StoryId, IntroText = draft.Intro });

                foreach (var q in draft.Questions)
                {
                    var qs = new QuestionScene
                    {
                        StoryId = story.StoryId,
                        SceneText = q.QuestionText,
                        Question  = q.QuestionText,
                        AnswerOptions = new List<AnswerOption>()
                    };

                    int n = Math.Min(q.Answers.Count, Math.Min(q.Feedbacks.Count, q.IsCorrect.Count));
                    for (int j = 0; j < n; j++)
                    {
                        qs.AnswerOptions.Add(new AnswerOption
                        {
                            Answer = q.Answers[j],
                            FeedbackText = q.Feedbacks[j],
                            IsCorrect = q.IsCorrect[j]
                        });
                    }
                    _db.QuestionScenes.Add(qs);
                }

                _db.EndingScenes.AddRange(
                    new EndingScene { StoryId = story.StoryId, EndingType = EndingType.Good,    EndingText = draft.HighEnding   },
                    new EndingScene { StoryId = story.StoryId, EndingType = EndingType.Neutral, EndingText = draft.MediumEnding },
                    new EndingScene { StoryId = story.StoryId, EndingType = EndingType.Bad,     EndingText = draft.LowEnding    }
                );

                await _db.SaveChangesAsync();

                // Ferdig: tøm utkast
                SaveDraft(draft); // valgfritt
                ClearDraft();

                if (story.Accessible == Accessibility.Private)
                    return RedirectToAction(nameof(GameCreated), new { storyId = story.StoryId });

                TempData["JustCreatedTitle"] = story.Title;
                TempData["JustCreatedCode"]  = story.GameCode;
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            // ----- Fallback -----
            ModelState.AddModelError("", "Unknown action.");
            SaveDraft(draft);
            return View("Create", draft);
        }

        // ---------------------- Viser bekreftelsesside etter lagring ----------------------
        [HttpGet]
        public async Task<IActionResult> GameCreated(int storyId)
        {
            var story = await _db.Stories.FindAsync(storyId);
            if (story == null)
                return NotFound();

            // Sender informasjon til viewet via ViewBag
            ViewBag.GameCode = story.GameCode;
            ViewBag.Title = story.Title;

            return View();
        }



        // uendret
        private async Task<string> GenerateUniqueCodeAsync()
        {
            var rnd = new Random();
            string code;
            do { code = rnd.Next(0, 100_000_000).ToString("D8"); }
            while (await _db.Stories.AnyAsync(s => s.GameCode == code));
            return code;
        }
    }
}
