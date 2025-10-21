using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Jam.ViewModels;
using Jam.Models;              // <-- viktig
using Jam.DAL.StoryDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.QuestionDAL;
using Jam.Models.Enums;     // <-- viktig

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
                Title           = vm.Title ?? "",
                Description     = vm.Description ?? "",      // se NOTE om IntroText under
                DifficultyLevel = vm.DifficultyLevel,
                Accessible      = vm.Accessibility
                // Code settes senere hvis du bytter til Private i UpdateStory,
                // eller generer her hvis du vil.
            };

        await _stories.CreateStory(game); // CreateStory: Task (void). EF setter StoryId på 'story'.

        // 2) (Valgfritt) Håndter IntroText
        // A) Enkelt: legg introen i Description, eller
        // B) Legg til en egen kolonne Story.IntroText og lagre vm.IntroText der, eller
        // C) Lag første "intro-spørsmål" senere — men IKKE bruk PlayingSession her.

        // 3) Videre til spørsmåls-siden
        return RedirectToAction(nameof(CreateQuestion), new
        {
            gameId = game.StoryId,
            questionIndex = 1
        });
        [HttpGet]
        IActionResult CreateQuestion(int storyId, int questionIndex = 1)
        {
            var vm = new CreateQuestionSceneViewModel
            {
                StoryId = storyId,
                PreviousSceneId = null, // første spørsmål har ingen forrige scene
                QuestionsMade = questionIndex - 1 // antall spørsmål laget så langt
            };

            return View(vm);
        }

}

     

   
     

    // Neste side i flyten (vis spørsmåls-skjema)
    
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