using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.Controllers;

[Route("Story/{storyId:int}/Scenes")]
public class SceneController : Controller
{
    private readonly AppDbContext _db;
    public SceneController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(int storyId)
    {
        var scenes = await _db.Scenes.Where(s => s.StoryId == storyId).ToListAsync();
        ViewBag.StoryId = storyId;
        return View(scenes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int storyId, int id)
    {
        var scene = await _db.Scenes.FirstOrDefaultAsync(s => s.SceneId == id && s.StoryId == storyId);
        if (scene == null) return NotFound();
        return View(scene);
    }

    [HttpGet("Create")]
    public IActionResult Create(int storyId) => View(new Scene { StoryId = storyId });

    [HttpPost("Create")]
    public async Task<IActionResult> Create(int storyId, Scene model)
    {
        if (!ModelState.IsValid) return View(model);
        model.StoryId = storyId;
        _db.Scenes.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { storyId });
    }

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int storyId, int id)
    {
        var scene = await _db.Scenes.FirstOrDefaultAsync(s => s.SceneId == id && s.StoryId == storyId);
        if (scene == null) return NotFound();
        return View(scene);
    }

    [HttpPost("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int storyId, int id, Scene model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { storyId, id = model.SceneId });
    }

    [HttpPost("{id:int}/Delete")]
    public async Task<IActionResult> Delete(int storyId, int id)
    {
        var scene = await _db.Scenes.FirstOrDefaultAsync(s => s.SceneId == id && s.StoryId == storyId);
        if (scene == null) return NotFound();
        _db.Remove(scene);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { storyId });
    }
}
