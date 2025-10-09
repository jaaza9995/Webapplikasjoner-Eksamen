using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jam.Models;

namespace Jam.Controllers;

public class UserController : Controller
{
    private readonly AppDbContext _db;
    public UserController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Users.ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        return View(u);
    }

    [HttpGet] public IActionResult Create() => View(new User());
    [HttpPost]
    public async Task<IActionResult> Create(User model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Users.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        return View(u);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(User model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = model.UserId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        _db.Remove(u);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
