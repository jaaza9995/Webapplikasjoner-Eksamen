using Microsoft.AspNetCore.Mvc;
using Jam.Models;
using Jam.DAL.UserDAL;

namespace Jam.Controllers;

// Håndterer CRUD for brukere via repository-laget (IUserRepository)
public class UserController : Controller
{
    private readonly IUserRepository _repo; // Tilgang til brukerdata gjennom DAL

    // DI: repository injiseres slik at kontrolleren ikke er avhengig av DbContext direkte
    public UserController(IUserRepository repo)
    {
        _repo = repo;
    }

    // LISTE: alle brukere
    [HttpGet]
    public IActionResult Index()
    {
        // NB: IUserRepository har ikke "GetAll" – i et ekte UI ville du laget en dedikert metode.
        // Midlertidig løsning: vis ingen liste her, eller hent via DbContext om du ønsker.
        // For å holde oss til repo-kontrakten returnerer vi tomt view nå.
        return View(new List<User>());
    }

    // DETALJER: én bruker
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var u = await _repo.GetUserById(id);
        if (u == null) return NotFound("Bruker ikke funnet.");
        return View(u);
    }

    // CREATE (GET): vis tomt skjema
    [HttpGet]
    public IActionResult Create()
    {
        return View(new User());
    }

    // CREATE (POST): opprett ny bruker
    [HttpPost]
    [ValidateAntiForgeryToken] // CSRF-beskyttelse
    public async Task<IActionResult> Create(User model)
    {
        // Enkle server-side valideringer
        if (!ModelState.IsValid) return View(model);

        // Unik brukernavn?
        if (await _repo.UsernameExists(model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Brukernavnet er allerede i bruk.");
            return View(model);
        }

        // Unik e-post? (valgfri)
        if (!string.IsNullOrWhiteSpace(model.Email) && await _repo.UserEmailExists(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "E-postadressen er allerede i bruk.");
            return View(model);
        }

        // Viktig i ekte app: PasswordHash bør være hash av passord fra et ViewModel (ikke råstreng i modellen).
        await _repo.CreateUser(model);
        return RedirectToAction(nameof(Index));
    }

    // EDIT (GET): vis redigeringsskjema
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var u = await _repo.GetUserById(id);
        if (u == null) return NotFound("Bruker ikke funnet.");
        return View(u);
    }

    // EDIT (POST): oppdater bruker
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User model)
    {
        if (!ModelState.IsValid) return View(model);

        var existing = await _repo.GetUserById(model.UserId);
        if (existing == null) return NotFound("Bruker ikke funnet.");

        // Sjekk unikhet kun hvis feltet er endret
        if (!string.Equals(existing.Username, model.Username, StringComparison.Ordinal) &&
            await _repo.UsernameExists(model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Brukernavnet er allerede i bruk.");
            return View(model);
        }

        if (!string.Equals(existing.Email ?? "", model.Email ?? "", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(model.Email) &&
            await _repo.UserEmailExists(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "E-postadressen er allerede i bruk.");
            return View(model);
        }

        // NB: I en ekte løsning bør passord endres via egen “Change Password”-flyt.
        await _repo.UpdateUser(model);
        return RedirectToAction(nameof(Details), new { id = model.UserId });
    }

    // DELETE (POST): slett bruker
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _repo.GetUserById(id);
        if (u == null) return NotFound("Bruker ikke funnet.");

        var ok = await _repo.DeleteUser(id);
        if (!ok) return BadRequest("Kunne ikke slette brukeren.");

        return RedirectToAction(nameof(Index));
    }
}
