/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.Controllers;

[Route("Scenes/{sceneId:int}/Questions")]
public class QuestionController : Controller
{
    private readonly AppDbContext _db;
    public QuestionController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(int sceneId)
        => View(await _db.Questions.Where(q => q.SceneId == sceneId).ToListAsync());

    [HttpGet("Create")]
    public IActionResult Create(int sceneId) => View(new Question { SceneId = sceneId });

    [HttpPost("Create")]
    public async Task<IActionResult> Create(int sceneId, Question model)
    {
        if (!ModelState.IsValid) return View(model);
        model.SceneId = sceneId;
        _db.Questions.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { sceneId });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int sceneId, int id)
    {
        var q = await _db.Questions.FirstOrDefaultAsync(x => x.QuestionId == id && x.SceneId == sceneId);
        if (q == null) return NotFound();
        return View(q);
    }

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int sceneId, int id)
    {
        var q = await _db.Questions.FirstOrDefaultAsync(x => x.QuestionId == id && x.SceneId == sceneId);
        if (q == null) return NotFound();
        return View(q);
    }

    [HttpPost("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int sceneId, int id, Question model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { sceneId, id = model.QuestionId });
    }

    [HttpPost("{id:int}/Delete")]
    public async Task<IActionResult> Delete(int sceneId, int id)
    {
        var q = await _db.Questions.FirstOrDefaultAsync(x => x.QuestionId == id && x.SceneId == sceneId);
        if (q == null) return NotFound();
        _db.Remove(q);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { sceneId });
    }
}
*/