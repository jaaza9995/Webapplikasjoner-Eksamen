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
using System.Text.Json;

namespace Jam.Controllers;

public class CreateGameController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly ISceneRepository _scenes;
    private readonly IAnswerOptionRepository _answers;

    public CreateGameController(
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
    public IActionResult Create()
    {
        TempData.Remove("CurrentModel");  // sørger for at vi starter på nytt hver gang
        var model = new EditStoryViewModel
        {
            Step = 1, // start alltid på første steg
            DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList(),
            AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList()
        };
        return View(model);
    }



    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
    };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EditStoryViewModel model, string action)
    {
        // Navigasjon mellom steg
        if (action == "Next" || action == "Back")
        {
            // Fjern valideringsfeil for å unngå blokkering ved mellomlagring
            ModelState.Clear();

            // 🔹 Hent tidligere modell hvis den finnes
            EditStoryViewModel? saved = null;
            if (TempData["CurrentModel"] is string oldJson)
                saved = JsonSerializer.Deserialize<EditStoryViewModel>(oldJson, JsonOptions);

            // 🔹 Slå sammen felter – behold alltid nyeste verdier
            if (saved != null)
            {
                saved.Title = string.IsNullOrWhiteSpace(model.Title) ? saved.Title : model.Title;
                saved.Description = string.IsNullOrWhiteSpace(model.Description) ? saved.Description : model.Description;
                saved.Intro = string.IsNullOrWhiteSpace(model.Intro) ? saved.Intro : model.Intro;
                saved.DifficultyLevel = model.DifficultyLevel != 0 ? model.DifficultyLevel : saved.DifficultyLevel;
                saved.Accessibility = model.Accessibility != 0 ? model.Accessibility : saved.Accessibility;

                // Questions
                if (model.Questions != null && model.Questions.Count > 0)
                    saved.Questions = model.Questions;

                // Endings
                saved.HighEnding = string.IsNullOrWhiteSpace(model.HighEnding) ? saved.HighEnding : model.HighEnding;
                saved.MediumEnding = string.IsNullOrWhiteSpace(model.MediumEnding) ? saved.MediumEnding : model.MediumEnding;
                saved.LowEnding = string.IsNullOrWhiteSpace(model.LowEnding) ? saved.LowEnding : model.LowEnding;

                model = saved;
            }

            // 🔹 Oppdater steg basert på handling
            if (action == "Next")
                model.Step = Math.Min(3, model.Step + 1);
            else if (action == "Back")
                model.Step = Math.Max(1, model.Step - 1);

            // 🔹 Initialiser lister for de riktige stegene
            if (model.Step == 2 && (model.Questions == null || model.Questions.Count == 0))
            {
                model.Questions = new List<CreateQuestionViewModel>
        {
            new CreateQuestionViewModel
            {
                QuestionText = "",
                Answers = new List<string> { "", "", "", "" },
                Feedbacks = new List<string> { "", "", "", "" },
                IsCorrect = new List<bool> { false, false, false, false }
            }
        };
            }
            else if (model.Step == 3)
            {
                model.HighEnding ??= "";
                model.MediumEnding ??= "";
                model.LowEnding ??= "";
            }

            // 🔹 Fjern dropdowns før lagring (kan ikke serialiseres)
            model.DifficultyOptions = null;
            model.AccessibilityOptions = null;

            // 🔹 Lagre modellen i TempData
            TempData["CurrentModel"] = JsonSerializer.Serialize(model, JsonOptions);
            TempData.Keep("CurrentModel");

            // 🔹 Gjør dropdowns klar igjen før visning
            RepopulateDropdowns(model);

            return View("Create", model);
        }

        // Tilbake til hovedview som velger partial basert på Step

        if (action == "AddQuestion")
        {
            // Hvis listen er null, opprett den
            if (model.Questions == null)
                model.Questions = new List<CreateQuestionViewModel>();

            // Legg til et nytt tomt spørsmål
            model.Questions.Add(new CreateQuestionViewModel
            {
                QuestionText = "",
                Answers = new List<string> { "", "", "", "" },
                Feedbacks = new List<string> { "", "", "", "" },
                IsCorrect = new List<bool> { false, false, false, false }
            });

            // behold brukeren på steg 2
            model.Step = 2;

            RepopulateDropdowns(model);
        }


        if (action == "Finish")
        {
            ModelState.Clear();
             Console.WriteLine("DEBUG: Finish ble trykket!");
            // Valider feltene for endings
            if (string.IsNullOrWhiteSpace(model.HighEnding))
                ModelState.AddModelError(nameof(model.HighEnding), "Good ending is required.");
            if (string.IsNullOrWhiteSpace(model.MediumEnding))
                ModelState.AddModelError(nameof(model.MediumEnding), "Neutral ending is required.");
            if (string.IsNullOrWhiteSpace(model.LowEnding))
                ModelState.AddModelError(nameof(model.LowEnding), "Bad ending is required.");

            if (!ModelState.IsValid)
            {
                model.Step = 3;
                RepopulateDropdowns(model);
                return View("Create", model);
            }

            try
            {
                // 1️⃣ Lagre story
                string? code = model.Accessibility == Accessibility.Private
                    ? Guid.NewGuid().ToString("N")[..8].ToUpper()
                    : null;

                
                var story = new Story
                {
                    Title = model.Title ?? "",
                    Description = model.Description ?? "",
                    DifficultyLevel = model.DifficultyLevel,
                    Accessible = model.Accessibility,
                    GameCode = code,
                    UserId = 1
                };

                var added = await _stories.AddStory(story);
                if (!added) return Problem("Klarte ikke å lagre historien.");

                // 2️⃣ Lagre questions og answers (din eksisterende logikk)
                if (model.Questions != null)
                {
                    foreach (var q in model.Questions)
                    {
                        var questionScene = new QuestionScene
                        {
                            StoryId = story.StoryId,
                            SceneText = q.QuestionText,
                            Question = q.QuestionText
                        };

                        var createdScene = await _scenes.AddQuestionScene(questionScene);
                        if (!createdScene) continue;

                        story.QuestionScenes ??= new List<QuestionScene>();
                        story.QuestionScenes.Add(questionScene);

                        if (q.Answers != null)
                        {
                            q.Feedbacks ??= new List<string>();
                            q.IsCorrect ??= new List<bool>();
                            while (q.Feedbacks.Count < q.Answers.Count) q.Feedbacks.Add("");
                            while (q.IsCorrect.Count < q.Answers.Count) q.IsCorrect.Add(false);

                            for (int i = 0; i < q.Answers.Count; i++)
                            {
                                var answer = new AnswerOption
                                {
                                    QuestionSceneId = questionScene.QuestionSceneId,
                                    Answer = q.Answers[i],
                                    FeedbackText = q.Feedbacks[i],
                                    IsCorrect = q.IsCorrect[i]
                                };
                                await _answers.AddAnswerOption(answer);
                            }
                        }
                    }
                }

                // 3️⃣ Lagre endings
                await _scenes.AddEndingScene(new EndingScene { StoryId = story.StoryId, EndingType = EndingType.Good, EndingText = model.HighEnding ?? "" });
                await _scenes.AddEndingScene(new EndingScene { StoryId = story.StoryId, EndingType = EndingType.Neutral, EndingText = model.MediumEnding ?? "" });
                await _scenes.AddEndingScene(new EndingScene { StoryId = story.StoryId, EndingType = EndingType.Bad, EndingText = model.LowEnding ?? "" });

                // ✅ Send bruker hjem igjen
                TempData["SuccessMessage"] = "Story created successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("Unexpected error during saving.");
            }

        }
    return View("Create", model);

    }
    
    [HttpGet]
    public async Task<IActionResult> CreateQuestions(int storyId)
    {
        // Hent historien og tilhørende spørsmål + svaralternativer
        var story = await _stories.GetStoryById(storyId);
        if (story == null) return NotFound();

        if (story == null) return NotFound();

        // Map til viewmodel
        var model = new CreateStoryViewModel
        {
            StoryId = story.StoryId,
            Title = story.Title,
            Description = story.Description,
            DifficultyLevel = story.DifficultyLevel,
            Accessibility = story.Accessible,
            GameCode = story.GameCode,
            Step = 2, // viser at vi er på spørsmål-steget
            Questions = story.QuestionScenes.Select(q => new CreateQuestionViewModel
            {
                QuestionId = q.QuestionSceneId,
                QuestionText = q.SceneText,
                Answers = q.AnswerOptions.Select(a => a.Answer).ToList(),
                Feedbacks = q.AnswerOptions.Select(a => a.FeedbackText ?? "").ToList(),
                IsCorrect = q.AnswerOptions.Select(a => a.IsCorrect).ToList()
            }).ToList(),
            DifficultyOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList(),
            AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList()
        };

        return View("_CreateQuestions", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestions(CreateQuestionViewModel model, int storyId)
    {
        if (!ModelState.IsValid)
            return View("_CreateQuestions", model);

        // Hent historien med eksisterende spørsmål
        var story = await _stories.GetStoryById(storyId);

        if (story == null) return NotFound();

        // Opprett nytt QuestionScene basert på ViewModel
        var questionScene = new QuestionScene
        {
            SceneText = "", // kan brukes til intro eller ekstra tekst om ønskelig
            Question = model.QuestionText,
            StoryId = story.StoryId,
            AnswerOptions = model.Answers.Select((answer, i) => new AnswerOption
            {
                Answer = answer,
                FeedbackText = model.Feedbacks[i],
                IsCorrect = model.IsCorrect[i]
            }).ToList()
        };
        await _scenes.AddQuestionScene(questionScene);
        foreach (var answer in questionScene.AnswerOptions)
            await _answers.AddAnswerOption(answer);


        // Legg til spørsmålet i storyen
        story.QuestionScenes.Add(questionScene);

        // Send brukeren tilbake for å legge til flere spørsmål
        return RedirectToAction(nameof(CreateQuestions), new { storyId = story.StoryId });
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
        await _scenes.AddQuestionScene(question);
        }



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
    public async Task<IActionResult> DeleteQuestionInline(EditStoryViewModel model, int deleteIndex)
    {
        if (model.Questions == null || model.Questions.Count == 0)
            return View("Create", model);

        model.Questions.RemoveAt(deleteIndex);
        model.Step = 2;
        RepopulateDropdowns(model);

        return View("Create", model);   // bruker fortsatt samme view
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

        if (!ModelState.IsValid)
            return View(model);

        // 💾 Lagre alle endings via SceneRepository
        await _scenes.AddEndingScene(new EndingScene
        {
            StoryId = model.StoryId,
            EndingType = EndingType.Good,
            EndingText = model.HighEnding
        });

        await _scenes.AddEndingScene(new EndingScene
        {
            StoryId = model.StoryId,
            EndingType = EndingType.Neutral,
            EndingText = model.MediumEnding
        });

        await _scenes.AddEndingScene(new EndingScene
        {
            StoryId = model.StoryId,
            EndingType = EndingType.Bad,
            EndingText = model.LowEnding
        });

        // 🔍 Hent historien via StoryRepository (ikke _db)
        var story = await _stories.GetStoryById(model.StoryId);

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
        var story = await _stories.GetStoryById(storyId);
        if (story == null) return NotFound();

        ViewBag.GameCode = story.GameCode;
        ViewBag.Title = story.Title;
        return View();
    }

}
