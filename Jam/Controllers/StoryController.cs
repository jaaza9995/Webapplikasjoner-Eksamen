using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.Controllers;

public class StoryController : Controller
{
    private readonly AppDbContext _db;
    public StoryController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Stories.Include(s => s.User).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var story = await _db.Stories.Include(s => s.User).FirstOrDefaultAsync(s => s.StoryId == id);
        if (story == null) return NotFound();
        return View(story);
    }

    [HttpGet] public IActionResult Create() => View(new Story());
    [HttpPost]
    public async Task<IActionResult> Create(Story model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Stories.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var story = await _db.Stories.FindAsync(id);
        if (story == null) return NotFound();
        return View(story);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(Story model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = model.StoryId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var story = await _db.Stories.FindAsync(id);
        if (story == null) return NotFound();
        _db.Remove(story);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
