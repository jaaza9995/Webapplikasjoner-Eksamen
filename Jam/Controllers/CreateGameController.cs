using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Jam.ViewModels;
using Jam.Models;
using Jam.DAL.SceneDAL;
using Jam.DAL.StoryDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.QuestionDAL;
using Jam.Models.Enums;


namespace Jam.Controllers;

public class CreateGameController : Controller
{
    private readonly IStoryRepository _stories;
    private readonly ISceneRepository _scenes;
    private readonly IQuestionRepository _questions;
    private readonly IAnswerOptionRepository _answers;
    //private readonly ILogger<CreateGameController> _logger;

    public CreateGameController(
        IAnswerOptionRepository answerOptionRepository,
        IQuestionRepository questionRepository,
        IStoryRepository storiesRepository,
        ISceneRepository scenesRepository)
        //ILogger<CreateGameController> logger)
    {
        _answers = answerOptionRepository;
        _questions = questionRepository;
        _stories = storiesRepository;
        _scenes = scenesRepository;


    }
    [HttpGet]
    public IActionResult CreateStoryAndIntro()
    {
        var vm = new CreateStoryAndIntroViewModel
        {
            DifficultyLevelOptions = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem
                {
                    Value = d.ToString(),     // evt: ((int)d).ToString()
                    Text  = d.ToString()      // evt: "Easy"/"Medium"/"Hard" på norsk
                })
                .ToList(),

            AccessibilityOptions = Enum.GetValues(typeof(Accessibility))
                .Cast<Accessibility>()
                .Select(a => new SelectListItem
                {
                    Value = a.ToString(),     // evt: ((int)a).ToString()
                    Text  = a.ToString()      // f.eks. "Public"/"Private"
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateStoryAndIntro(CreateStoryAndIntroViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Viktig: repopuler listene ved valideringsfeil (akkurat som i MyShop)
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

            // TODO: lagre story + intro, så gå videre
            return RedirectToAction(nameof(CreateScene), new { /* storyId = ... */ });
    }

    [HttpGet]
    public IActionResult CreateScene(int storyId) {
        return View();
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