using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Jam.ViewModels;
using Jam.Models;    
using Jam.DAL.StoryDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.Models.Enums;
using Jam.DAL;
using Jam.DAL.SceneDAL;

namespace Jam.Controllers;

public class CreateGameController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly ISceneRepository _scenes;
    private readonly IAnswerOptionRepository _answers;
    private readonly StoryDbContext _db;

    public CreateGameController(
        IAnswerOptionRepository answerOptionRepository,
        IStoryRepository storiesRepository,
        ISceneRepository sceneRepository,
        StoryDbContext db)
    {
        _answers = answerOptionRepository;
        _stories = storiesRepository;
        _scenes = sceneRepository;
        _db = db;
    }


    [HttpGet]
    public IActionResult CreateIntro()
    {
        var CreateIntroVM = new CreateStoryViewModel
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
        return View("~/Views/Home/Create.cshtml", CreateIntroVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateIntro(CreateStoryViewModel CreateStoryVM)
    {
        if (!ModelState.IsValid)
        {
            // repopuler dropdowns ved valideringsfeil
            CreateStoryVM.DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList();
            CreateStoryVM.AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList();

            return View(CreateStoryVM);
        }

        // 1) Generer kode hvis spillet er privat
        string? gameCode = null;
        if (CreateStoryVM.Accessibility == Accessibility.Private)
            gameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

        // 2) Lagre storyen
        var game = new Story
        {
            Title = CreateStoryVM.Title ?? "",
            Description = CreateStoryVM.Description ?? "",
            DifficultyLevel = CreateStoryVM.DifficultyLevel,
            Accessible = CreateStoryVM.Accessibility,
            GameCode = gameCode
        };

        await _stories.AddStory(game);

        // 3) Gå videre til neste steg
        return RedirectToAction(nameof(CreateMultipleQuestion), new
        {
            gameId = game.StoryId,
            questionIndex = 1
        });
    }


    // GET: /CreateGame/CreateQuestion?storyId=123&questionIndex=1
// using Jam.Models.Enums;  // bare hvis du filtrerer på SceneType andre steder
[HttpGet]
public async Task<IActionResult> CreateMultipleQuestion(int storyId, int questionIndex = 1)
{
    var qScenes = await _scenes.GetQuestionScenesByStoryId(storyId); // returnerer IEnumerable<QuestionScene>

    var vm = new CreateStoryViewModel
    {
        StoryId = storyId,
        Step = questionIndex,
        Questions = qScenes.Select(qs =>
        {
            // Hent lagrede svar (hvis du har AnswerOptions på QuestionScene)
            var answers   = qs.AnswerOptions?.Select(a => a.Answer).ToList() ?? new List<string>();
            var feedbacks = qs.AnswerOptions?.Select(a => a.FeedbackText ?? "").ToList() ?? new List<string>();
            var correct   = qs.AnswerOptions?.Select(a => a.IsCorrect).ToList() ?? new List<bool>();

            // Pad til 4 elementer (UI-en din forventer 4)
            while (answers.Count   < 4) answers.Add("");
            while (feedbacks.Count < 4) feedbacks.Add("");
            while (correct.Count   < 4) correct.Add(false);

            return new CreateQuestionViewModel
            {
                QuestionId   = qs.QuestionSceneId,                // riktig id for redigering
                QuestionText = qs.SceneText ?? string.Empty,      // evt. qs.QuestionText hvis det er feltet ditt
                Answers      = answers,
                Feedbacks    = feedbacks,
                IsCorrect    = correct
            };
        }).ToList()
    };

    if (vm.Questions.Count == 0)
        vm.Questions.Add(new CreateQuestionViewModel()); // første gang: vis tomt skjema

    return View(vm);
}

        

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMultipleQuestion(CreateStoryViewModel model, string? submit)
    {
        if (model == null) return BadRequest("Ugyldig skjema.");

        // Valider hvert spørsmål i modellen
    for (int i = 0; i < model.Questions.Count; i++)
    {
        var q = model.Questions[i];

        if (string.IsNullOrWhiteSpace(q.QuestionText))
            ModelState.AddModelError($"Questions[{i}].QuestionText", "Spørsmål er påkrevd.");

        if (q.Answers == null || q.Answers.Count != 4)
            ModelState.AddModelError($"Questions[{i}].Answers", "Du må ha nøyaktig 4 svaralternativer.");

        if (q.IsCorrect == null || q.IsCorrect.Count != 4)
        ModelState.AddModelError($"Questions[{i}].IsCorrect", "Du må ha 4 verdier i IsCorrect-listen.");

        var correctCount = q.IsCorrect.Count(c => c); // teller antall true
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

    if (!ModelState.IsValid)
        return View(model);

    // Lagre alle spørsmål i databasen
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
    await _db.SaveChangesAsync();
    }

        // Navigasjon videre
        model.Step++;
        if (string.Equals(submit, "next", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CreateMultipleQuestion), new { storyId = model.StoryId, questionIndex = model.Step });
        }

        if (model.Questions.Count >= 3)
        {
            return RedirectToAction(nameof(CreateEndings), new { storyId = model.StoryId });
        }

        return RedirectToAction(nameof(CreateMultipleQuestion), new { storyId = model.StoryId, questionIndex = model.Step });
    }

        // Neste side i flyten (vis spørsmåls-skjema)

    [HttpGet]
    public IActionResult CreateEndings(int storyId)
    {
        return View(new CreateStoryViewModel { StoryId = storyId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEndings(CreateStoryViewModel CreateStoryVM)
    {
        if (string.IsNullOrWhiteSpace(CreateStoryVM.HighEnding))
            ModelState.AddModelError(nameof(CreateStoryVM.HighEnding), "Good ending er påkrevd.");
        if (string.IsNullOrWhiteSpace(CreateStoryVM.MediumEnding))
            ModelState.AddModelError(nameof(CreateStoryVM.MediumEnding), "Neutral ending er påkrevd.");
        if (string.IsNullOrWhiteSpace(CreateStoryVM.LowEnding))
            ModelState.AddModelError(nameof(CreateStoryVM.LowEnding), "Bad ending er påkrevd.");

        if (!ModelState.IsValid) return View(CreateStoryVM);


        // Lag tre avslutningsscener
        var high = new EndingScene
        {
            StoryId = CreateStoryVM.StoryId,
            EndingType = EndingType.Good,
            EndingText = CreateStoryVM.HighEnding
        };

        var medium = new EndingScene
        {
            StoryId = CreateStoryVM.StoryId,
            EndingType = EndingType.Neutral,
            EndingText = CreateStoryVM.MediumEnding
        };

        var low = new EndingScene
        {
            StoryId = CreateStoryVM.StoryId,
            EndingType = EndingType.Bad,
            EndingText = CreateStoryVM.LowEnding
        };

        // Legg dem til databasen via DbContext
        _db.EndingScenes.AddRange(high, medium, low);
        await _db.SaveChangesAsync();

        var story = await _db.Stories.FindAsync(CreateStoryVM.StoryId);
        if (story?.Accessible == Accessibility.Private && !string.IsNullOrEmpty(story.GameCode))
        {
            TempData["GameCode"] = story.GameCode;
            return RedirectToAction(nameof(GameCreated), new { storyId = story.StoryId });
        }

        // Ellers send tilbake til Home som før
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
    
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateOrEdit(CreateStoryViewModel model, string action)
{
    // Navigering mellom steg
    if (string.Equals(action, "Back", StringComparison.OrdinalIgnoreCase))
    {
        model.Step = Math.Max(1, model.Step - 1);
        return View("~/Views/Home/Create.cshtml", model);
    }
    if (string.Equals(action, "Next", StringComparison.OrdinalIgnoreCase))
    {
        model.Step++;
        return View("~/Views/Home/Create.cshtml", model);
    }

    // Finish = lagre
    if (!ModelState.IsValid)
        return View("~/Views/Home/Create.cshtml", model);

    if (model is EditStoryViewModel { IsEditMode: true })
    {
        // UPDATE
        var story = await _db.Stories.FindAsync(model.StoryId);
        if (story == null) return NotFound();

        story.Title = model.Title ?? "";
        story.Description = model.Description ?? "";
        //story.Intro = model.Intro ?? "";
        story.DifficultyLevel = model.DifficultyLevel;
        // Håndter endring av public/private + kode
        if (story.Accessible != model.Accessibility)
        {
            story.Accessible = model.Accessibility;
            if (story.Accessible == Accessibility.Private && string.IsNullOrEmpty(story.GameCode))
                story.GameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
            if (story.Accessible == Accessibility.Public)
                story.GameCode = null; // valgfritt
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Details", "Story", new { id = story.StoryId });
    }
    else
    {
        // CREATE
        string? code = null;
        if (model.Accessibility == Accessibility.Private)
            code = Guid.NewGuid().ToString("N")[..6].ToUpper();

        var story = new Story
        {
            Title = model.Title ?? "",
            Description = model.Description ?? "",
            //IntroScene = model.intro ?? "",
            DifficultyLevel = model.DifficultyLevel,
            Accessible = model.Accessibility,
            GameCode = code
        };

        await _stories.AddStory(story);
        // Videre til spørsmål-flyten din
        return RedirectToAction(nameof(CreateMultipleQuestion), new { storyId = story.StoryId, questionIndex = 1 });
    }
}

}