using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Jam.ViewModels;
using Jam.Models;             
using Jam.DAL.StoryDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.QuestionDAL;
using Jam.Models.Enums;     

namespace Jam.Controllers;

public class CreateGameController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly IQuestionRepository _questions;
    private readonly IAnswerOptionRepository _answers;

    public CreateGameController(
        IAnswerOptionRepository answerOptionRepository,
        IQuestionRepository questionRepository,
        IStoryRepository storiesRepository)
    {
        _answers = answerOptionRepository;
        _questions = questionRepository;
        _stories = storiesRepository;
    }

    [HttpGet]
    public IActionResult CreateStoryAndIntro()
    {
        var vm = new CreateStoryAndIntroViewModel
        {
            DifficultyLevelOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList(),
            AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStoryAndIntro(CreateStoryAndIntroViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            // repopuler dropdowns ved valideringsfeil
            vm.DifficultyLevelOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() })
                .ToList();
            vm.AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem { Value = a.ToString(), Text = a.ToString() })
                .ToList();

            return View(vm);
        }

        // 1) Lagre selve Story (kun StoryRepository)
        var game = new Story
        {
            Title = vm.Title ?? "",
            Description = vm.Description ?? "",      // se NOTE om IntroText under
            DifficultyLevel = vm.DifficultyLevel,
            Accessible = vm.Accessibility
            // Code settes senere hvis du bytter til Private i UpdateStory,
            // eller generer her hvis du vil.
        };

        await _stories.CreateStory(game); // CreateStory: Task (void). EF setter StoryId på 'story'.

        return RedirectToAction(nameof(CreateQuestionScene), new
        {
            gameId = game.StoryId,
            questionIndex = 1
        });
    }

        // GET: /CreateGame/CreateQuestion?storyId=123&questionIndex=1
        [HttpGet]
        public IActionResult CreateQuestionScene(int storyId, int questionIndex = 1)
        {
            var vm = new CreateQuestionSceneViewModel
            {
                StoryId = storyId,
                PreviousSceneId = null,            // kan kobles senere om ønsket
                QuestionsMade = Math.Max(0, questionIndex - 1)
            };
            return View(vm);
        }

        // POST: /CreateGame/CreateQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(CreateQuestionSceneViewModel vm, string? submit)
        {
            // Enkle servervalideringer:
            if (vm == null) return BadRequest("Ugyldig skjema.");
            if (string.IsNullOrWhiteSpace(vm.StoryText))
                ModelState.AddModelError(nameof(vm.StoryText), "Story context er påkrevd.");
            if (string.IsNullOrWhiteSpace(vm.QuestionText))
                ModelState.AddModelError(nameof(vm.QuestionText), "Spørsmål er påkrevd.");
            if (vm.Answers == null || vm.Answers.Count != 4)
                ModelState.AddModelError(nameof(vm.Answers), "Du må ha nøyaktig 4 svaralternativer.");

            // Kun 1 riktig (i notatene dine besluttet dere dette)
            var correctCount = vm.Answers?.Count(a => a.IsCorrect) ?? 0;
            if (correctCount != 1)
                ModelState.AddModelError(nameof(vm.Answers), "Nøyaktig ett alternativ må være riktig.");

            // Svar- og outcome-tekst må være utfylt
            if (vm.Answers != null)
            {
                for (int i = 0; i < vm.Answers.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(vm.Answers[i].AnswerText))
                        ModelState.AddModelError($"Answers[{i}].AnswerText", "Svartekst er påkrevd.");
                    if (string.IsNullOrWhiteSpace(vm.Answers[i].ContextText))
                        ModelState.AddModelError($"Answers[{i}].ContextText", "Outcome/feedback-tekst er påkrevd.");
                }
            }

            if (!ModelState.IsValid)
            {
                // vis samme boks igjen med valideringsfeil
                return View(vm);
            }

            // Bygg Question med innebygd Scene og 4 AnswerOptions (lar EF cascade-lagre alt i ett kall)
            var question = new Question
            {
                QuestionText = vm.QuestionText,
                Scene = new Scene
                {
                    SceneType = SceneType.Question,
                    SceneText = vm.StoryText,   // "story context"-boksen
                    StoryId = vm.StoryId
                },
                AnswerOptions = vm.Answers.Select(a => new AnswerOption
                {
                    Answer = a.AnswerText,
                    SceneText = a.ContextText,  // outcome-teksten som egen "mini-scene"
                    IsCorrect = a.IsCorrect
                }).ToList()
            };

            await _questions.CreateQuestion(question); // lagrer Scene, Question og AnswerOptions i ett (cascade)

            // TODO (valgfritt): lenke forrige scene -> denne scenen.
            // Krever SceneRepository eller DbContext for å sette Previous.NextSceneId = question.Scene.SceneId.

            // Navigasjon videre
            int nextIndex = vm.QuestionsMade + 1;

            if (string.Equals(submit, "next", StringComparison.OrdinalIgnoreCase))
            {
                // Ny spørsmålsboks
                return RedirectToAction(nameof(CreateQuestion), new { storyId = vm.StoryId, questionIndex = nextIndex + 1 });
            }

            // Hvis vi har minst 3 spørsmål, gå til endings-side; ellers send til ny boks uansett
            if (nextIndex >= 3)
                return RedirectToAction(nameof(CreateEndings), new { storyId = vm.StoryId });

            return RedirectToAction(nameof(CreateQuestion), new { storyId = vm.StoryId, questionIndex = nextIndex + 1 });
        }


        

        // Neste side i flyten (vis spørsmåls-skjema)

        [HttpGet]
        public IActionResult CreateEndings(int storyId)
        {
            return View(new CreateEndingsViewModel { StoryId = storyId });
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEndings(CreateEndingsViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.GoodEnding))
                ModelState.AddModelError(nameof(vm.GoodEnding), "Good ending er påkrevd.");
            if (string.IsNullOrWhiteSpace(vm.NeutralEnding))
                ModelState.AddModelError(nameof(vm.NeutralEnding), "Neutral ending er påkrevd.");
            if (string.IsNullOrWhiteSpace(vm.BadEnding))
                ModelState.AddModelError(nameof(vm.BadEnding), "Bad ending er påkrevd.");

            if (!ModelState.IsValid) return View(vm);

            // Lag tre endingscener (bruk din SceneRepository eller DbContext)
            var good = new Scene { StoryId = vm.StoryId, SceneType = SceneType.EndingGood, SceneText = vm.GoodEnding };
            var neutral = new Scene { StoryId = vm.StoryId, SceneType = SceneType.EndingNeutral, SceneText = vm.NeutralEnding };
            var bad = new Scene { StoryId = vm.StoryId, SceneType = SceneType.EndingBad, SceneText = vm.BadEnding };

            // Eksempel dersom du har en _db (StoryDbContext) tilgjengelig; ellers kall _scenes.CreateScene(...)
            // _db.Scenes.AddRange(good, neutral, bad);
            // await _db.SaveChangesAsync();

            // Ferdig → hjem
            return RedirectToAction("Index", "Home");
        }


    }


    /*

    public async Task<IActionResult> Table()
    {
        var items = await GetPlayingSessionById.GetAll();
        if (items == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return NotFound("Item list not found");
        }
        var itemsViewModel = new ItemsViewModel(items, "Table");
        return View(itemsViewModel);
    }

    public async Task<IActionResult> Grid()
    {
        var items = await _itemRepository.GetAll();
        if (items == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return NotFound("Item list not found");
        }
        var itemsViewModel = new ItemsViewModel(items, "Grid");
        return View(itemsViewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _itemRepository.GetItemById(id);
        if (item == null)
        {
            _logger.LogError("[ItemController] Item not found for the ItemId {ItemId:0000}", id);
            return NotFound("Item not found for the ItemId");
        }
        return View(item);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Item item)
    {
        if (ModelState.IsValid)
        {
            bool returnOk = await _itemRepository.Create(item);
            if (returnOk)
                return RedirectToAction(nameof(Table));
        }
        _logger.LogWarning("[ItemController] Item creation failed {@item}", item);
        return View(item);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Update(int id)
    {
        var item = await _itemRepository.GetItemById(id);
        if (item == null)
        {
            _logger.LogError("[ItemController] Item not found when updating the ItemId {ItemId:0000}", id);
            return BadRequest("Item not found for the ItemId");
        }
        return View(item);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Update(Item item)
    {
        if (ModelState.IsValid)
        {
            bool returnOk = await _itemRepository.Update(item);
            if (returnOk)
                return RedirectToAction(nameof(Table));
        }
        _logger.LogWarning("[ItemController] Item update failed {@item}", item);
        return View(item);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _itemRepository.GetItemById(id);
        if (item == null)
        {
            _logger.LogError("[ItemController] Item not found for the ItemId {ItemId:0000}", id);
            return BadRequest("Item not found for the ItemId");
        }
        return View(item);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool returnOk = await _itemRepository.Delete(id);
        if (!returnOk)
        {
            _logger.LogError("[ItemController] Item deletion failed for the ItemId {ItemId:0000}", id);
            return BadRequest("Item deletion failed");
        }
        return RedirectToAction(nameof(Table));
    }
}
*/