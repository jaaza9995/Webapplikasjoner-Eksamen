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

public class CreateGameLoggingController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly ISceneRepository _scenes;
    private readonly IAnswerOptionRepository _answers;
    private readonly StoryDbContext _db;
    //private readonly ILogger<CreateGameController> _logger;

    public CreateGameLoggingController(
        IAnswerOptionRepository answerOptionRepository,
        IStoryRepository storiesRepository,
        ISceneRepository sceneRepository,
        StoryDbContext db)
    //ILogger<CreateGameController> logger) 
    {
        _answers = answerOptionRepository;
        _stories = storiesRepository;
        _scenes = sceneRepository;
        _db = db;
        //_logger = logger;
    }



    [HttpGet]
    public IActionResult CreateIntro()
    {
        var vm = new EditStoryViewModel
        {
            DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList(),
            AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList()
        };
        return View("~/Views/Home/Create.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit(EditStoryViewModel model, string action)
    {
        // Navigasjon
        if (string.Equals(action, "Back", StringComparison.OrdinalIgnoreCase))
        {
            model.Step = Math.Max(1, model.Step - 1);
            return View("~/Views/Home/Create.cshtml", model);
        }
        else if (string.Equals(action, "Next", StringComparison.OrdinalIgnoreCase))
        {
            model.Step++;
            return View("~/Views/Home/Create.cshtml", model);
        }
        else if (string.Equals(action, "Finish", StringComparison.OrdinalIgnoreCase))
        {
            if (!ModelState.IsValid)
            {
                // repopuler dropdowns
                model.DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList();

            model.AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList();

                return View("~/Views/Home/Create.cshtml", model);
            }

            try
            {
                if (model.IsEditMode)
                {
                    var story = await _db.Stories.FindAsync(model.StoryId);
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

                    await _db.SaveChangesAsync();
                    return RedirectToAction("Details", "Story", new { id = story.StoryId });
                }
                else
                {
                    string? code = model.Accessibility == Accessibility.Private
                        ? Guid.NewGuid().ToString("N")[..6].ToUpper()
                        : null;

                    var story = new Story
                    {
                        Title = model.Title ?? "",
                        Description = model.Description ?? "",
                        DifficultyLevel = model.DifficultyLevel,
                        Accessible = model.Accessibility,
                        GameCode = code
                    };

                    await _stories.AddStory(story);
                    return RedirectToAction(nameof(CreateMultipleQuestion),
                        new { storyId = story.StoryId, questionIndex = 1 });
                }
            }
            catch (Exception)
            {
                return Problem("Uventet feil under lagring.");
            }
        }

        // Fallback: ukjent action → returner view med feilmelding (hindrer CS0161)
        ModelState.AddModelError(string.Empty, "Ukjent handling.");
        return View("~/Views/Home/Create.cshtml", model);
    }





    // GET: /CreateGame/CreateMultipleQuestion?storyId=123&questionIndex=1
    [HttpGet]
    public async Task<IActionResult> CreateMultipleQuestion(int storyId, int questionIndex = 1)
    {
        var scenes = await _scenes.GetQuestionScenesByStoryId(storyId);

        var vm = new CreateStoryViewModel
        {
            StoryId = storyId,
            Step = questionIndex,
            Questions = scenes.Select(qs =>
            {
                var answers = qs.AnswerOptions?.Select(a => a.Answer).ToList() ?? new List<string>();
                var feedbacks = qs.AnswerOptions?.Select(a => a.FeedbackText ?? "").ToList() ?? new List<string>();
                var correct = qs.AnswerOptions?.Select(a => a.IsCorrect).ToList() ?? new List<bool>();

                while (answers.Count < 4) answers.Add("");
                while (feedbacks.Count < 4) feedbacks.Add("");
                while (correct.Count < 4) correct.Add(false);

                return new CreateQuestionViewModel
                {
                    QuestionId = qs.QuestionSceneId,
                    QuestionText = qs.SceneText ?? string.Empty,
                    Answers = answers,
                    Feedbacks = feedbacks,
                    IsCorrect = correct
                };
            }).ToList()
        };

        if (vm.Questions.Count == 0)
            vm.Questions.Add(new CreateQuestionViewModel());

        return View("~/Views/Home/Create.cshtml", vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMultipleQuestion(CreateStoryViewModel model, string? submit)
    {
        if (model == null) return BadRequest("Ugyldig skjema.");

        // Valider spørsmålene
        for (int i = 0; i < model.Questions.Count; i++)
        {
            var q = model.Questions[i];

            if (string.IsNullOrWhiteSpace(q.QuestionText))
                ModelState.AddModelError($"Questions[{i}].QuestionText", "Spørsmål er påkrevd.");

            if (q.Answers == null || q.Answers.Count != 4)
                ModelState.AddModelError($"Questions[{i}].Answers", "Du må ha nøyaktig 4 svaralternativer.");

            if (q.IsCorrect == null || q.IsCorrect.Count != 4)
                ModelState.AddModelError($"Questions[{i}].IsCorrect", "Du må ha 4 verdier i IsCorrect-listen.");

            var correctCount = q.IsCorrect.Count(c => c);
            if (correctCount != 1)
                ModelState.AddModelError($"Questions[{i}].IsCorrect", "Nøyaktig ett alternativ må være riktig.");

            for (int j = 0; j < q.Answers.Count; j++)
            {
                if (string.IsNullOrWhiteSpace(q.Answers[j]))
                    ModelState.AddModelError($"Questions[{i}].Answers[{j}]", "Svartekst er påkrevd.");

                if (string.IsNullOrWhiteSpace(q.Feedbacks[j]))
                    ModelState.AddModelError($"Questions[{i}].Feedbacks[{j}]", "Feedback-tekst er påkrevd.");
            }
        }

        // Fyll alltid ut tomme felter igjen (så input vises)
        for (int i = 0; i < model.Questions.Count; i++)
        {
            var q = model.Questions[i];
            q.Answers ??= new List<string>();
            q.Feedbacks ??= new List<string>();
            q.IsCorrect ??= new List<bool>();

            while (q.Answers.Count < 4) q.Answers.Add("");
            while (q.Feedbacks.Count < 4) q.Feedbacks.Add("");
            while (q.IsCorrect.Count < 4) q.IsCorrect.Add(false);
        }

        // Hvis det finnes valideringsfeil → vis samme view
        if (!ModelState.IsValid)
            return View("~/Views/Home/Create.cshtml", model);

        // Lagre spørsmålene (hvis ønskelig)
        foreach (var q in model.Questions)
        {
            var question = new QuestionScene
            {
                SceneText = q.QuestionText,
                Question = q.QuestionText,
                StoryId = model.StoryId,
                AnswerOptions = q.Answers.Select((a, idx) => new AnswerOption
                {
                    Answer = a,
                    FeedbackText = q.Feedbacks[idx],
                    IsCorrect = q.IsCorrect[idx]
                }).ToList()
            };
            _db.QuestionScenes.Add(question);
        }

        await _db.SaveChangesAsync();

        // Hvis brukeren trykker på "Next question"
        if (submit == "Next")
        {
            // legg til en ny tom spørsmålsboks
            model.Questions.Add(new CreateQuestionViewModel
            {
                Answers = new List<string> { "", "", "", "" },
                Feedbacks = new List<string> { "", "", "", "" },
                IsCorrect = new List<bool> { false, false, false, false }
            });

            // behold samme view (ingen redirect)
            return View("~/Views/Home/Create.cshtml", model);
        }

        // Hvis brukeren har nådd 3 spørsmål, gå til endings
        if (model.Questions.Count >= 3)
        {
            return RedirectToAction(nameof(CreateEndings), new { storyId = model.StoryId });
        }

        // Standard redirect (om man trykker på "Next step" f.eks.)
        return RedirectToAction(nameof(CreateMultipleQuestion),
            new { storyId = model.StoryId, questionIndex = model.Step });
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int storyId, int questionSceneId, int questionIndex)
    {
        // Finn spørsmål + relaterte svaralternativer
        var qs = await _db.QuestionScenes
            .Include(x => x.AnswerOptions)
            .FirstOrDefaultAsync(x => x.QuestionSceneId == questionSceneId && x.StoryId == storyId);

        if (qs == null)
        {
            TempData["Warn"] = "Finner ikke spørsmålet.";
            return RedirectToAction(nameof(CreateMultipleQuestion), new { storyId, questionIndex });
        }

        // Slett (EF vil slette AnswerOptions via cascade hvis konfigurert – ellers fjern dem eksplisitt)
        //_db.AnswerOptions.RemoveRange(qs.AnswerOptions); - om det ikke er cascade delete mellom QuestionScene → AnswerOption
        _db.QuestionScenes.Remove(qs);
        await _db.SaveChangesAsync();

        TempData["Info"] = "Spørsmålet ble slettet.";

        // Juster index hvis vi slettet siste element
        var totalEtter = await _db.QuestionScenes.CountAsync(x => x.StoryId == storyId);
        var nextIndex = Math.Min(Math.Max(1, questionIndex), Math.Max(1, totalEtter));

        return RedirectToAction(nameof(CreateMultipleQuestion), new { storyId, questionIndex = nextIndex });
    }


    [HttpGet]
    public IActionResult CreateEndings(int storyId)
    {
        return View(new CreateStoryViewModel { StoryId = storyId });
    }
        
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEndings(CreateStoryViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.HighEnding))
            ModelState.AddModelError(nameof(model.HighEnding), "Good ending er påkrevd.");
        if (string.IsNullOrWhiteSpace(model.MediumEnding))
            ModelState.AddModelError(nameof(model.MediumEnding), "Neutral ending er påkrevd.");
        if (string.IsNullOrWhiteSpace(model.LowEnding))
            ModelState.AddModelError(nameof(model.LowEnding), "Bad ending er påkrevd.");

        if (!ModelState.IsValid) return View(model);

        var high = new EndingScene { StoryId = model.StoryId, EndingType = EndingType.Good, EndingText = model.HighEnding };
        var medium = new EndingScene { StoryId = model.StoryId, EndingType = EndingType.Neutral, EndingText = model.MediumEnding };
        var low = new EndingScene { StoryId = model.StoryId, EndingType = EndingType.Bad, EndingText = model.LowEnding };

        _db.EndingScenes.AddRange(high, medium, low);
        await _db.SaveChangesAsync();

        var story = await _db.Stories.FindAsync(model.StoryId);
        if (story?.Accessible == Accessibility.Private && !string.IsNullOrEmpty(story.GameCode))
        {
            TempData["GameCode"] = story.GameCode;
            return RedirectToAction(nameof(GameCreated), new { storyId = story.StoryId });


        }

        return RedirectToAction("Index", "Home");
    }
        
        [HttpGet]
        public async Task<IActionResult> GameCreated(int storyId)
        {
            var story = await _db.Stories.FindAsync(storyId);
            if (story == null) return NotFound();

            ViewBag.GameCode = story.GameCode;
            ViewBag.Title = story.Title;
            return View();
        }

    }

