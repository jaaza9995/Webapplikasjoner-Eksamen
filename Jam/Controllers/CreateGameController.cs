using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Jam.ViewModels;
using Jam.Models;    
using Jam.DAL.StoryDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.Models.Enums;     
using Jam.DAL;

namespace Jam.Controllers;

public class CreateGameController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly IAnswerOptionRepository _answers;
    private readonly StoryDbContext _db;

    public CreateGameController(
        IAnswerOptionRepository answerOptionRepository,
        IStoryRepository storiesRepository,
        StoryDbContext db)
    {
        _answers = answerOptionRepository;
        _stories = storiesRepository;
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
        return View(CreateIntroVM);        
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
            Code = gameCode
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
       [HttpGet]
        public IActionResult CreateMultipleQuestion(int storyId, int questionIndex = 1)
        {
            var vm = new CreateStoryViewModel
            {
                StoryId = storyId,
                Step = questionIndex, // følger samme logikk som ditt “step”-felt
                Questions = new List<CreateQuestionViewModel>
                {
                    new CreateQuestionViewModel
                    {
                        QuestionId = 0,
                        QuestionText = string.Empty
                    }
                }
            };

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
        if (story?.Accessible == Accessibility.Private && !string.IsNullOrEmpty(story.Code))
        {
            TempData["GameCode"] = story.Code;
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

        ViewBag.Code = story.Code;
        ViewBag.Title = story.Title;
        return View();
    }
}