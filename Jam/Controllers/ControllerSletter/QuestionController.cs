/*using Microsoft.AspNetCore.Mvc;
using Jam.Models;
using Jam.DAL.QuestionDAL;

namespace Jam.Controllers;


[Route("Scenes/{sceneId:int}/Questions")]
public class QuestionController : Controller
{
    private readonly IQuestionRepository _repo;

    public QuestionController(IQuestionRepository repo)
    {
        _repo = repo; 
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int sceneId)
    {
        var all = await _repo.GetAllQuestions();
        var list = all.Where(q => q.SceneId == sceneId).ToList();
        return View(list);
    }


   
    [HttpGet("Create")]
    public IActionResult Create(int sceneId)
    {
        var question = new Question { SceneId = sceneId };

        return View(question);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int sceneId, Question model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.SceneId = sceneId;

        await _repo.CreateQuestion(model);

        return RedirectToAction(nameof(Index), new { sceneId });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int sceneId, int id)
    {
        var q = await _repo.GetQuestionById(id);

        if (q == null || q.SceneId != sceneId)
            return NotFound("Spørsmål ikke funnet.");

        return View(q);
    }

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int sceneId, int id)
    {
        var q = await _repo.GetQuestionById(id);

        if (q == null || q.SceneId != sceneId)
            return NotFound("Spørsmål ikke funnet.");

        return View(q);
    }

    [HttpPost("{id:int}/Edit")]
    [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Edit(int sceneId, int id, Question model)
    {
        if (id != model.QuestionId)
            return BadRequest("ID mismatch.");

        if (model.SceneId != sceneId)
            return BadRequest("Scene-ID mismatch.");

        if (!ModelState.IsValid)
            return View(model);

        await _repo.UpdateQuestion(model);

        return RedirectToAction(nameof(Details), new { sceneId, id = model.QuestionId });
    }


    [HttpPost("{id:int}/Delete")]
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> Delete(int sceneId, int id)
    {
        var q = await _repo.GetQuestionById(id);
        if (q == null || q.SceneId != sceneId)
            return NotFound("Spørsmål ikke funnet.");

        var ok = await _repo.DeleteQuestion(id);
        if (!ok)
            return BadRequest("Kunne ikke slette.");

        return RedirectToAction(nameof(Index), new { sceneId });
    }
}*/