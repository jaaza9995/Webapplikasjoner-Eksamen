/*using Microsoft.AspNetCore.Mvc;
using Jam.Models;
using Jam.DAL.StoryDAL;

namespace Jam.Controllers;

// Håndterer CRUD (Create, Read, Update, Delete) for Story-objekter
public class StoryController : Controller
{
    private readonly IStoryRepository _repo;
    // Felt som brukes for å kommunisere med databasen via repository-laget

    public StoryController(IStoryRepository repo)
    {
        _repo = repo;
    }

    // Viser en liste over alle historier
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stories = await _repo.GetAllStories();
        return View(stories);
    }

    // Viser detaljene for én bestemt historie
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // Henter historien fra databasen via repositoryet basert på ID
        var story = await _repo.GetStoryById(id);

        // Hvis ingen historie med denne ID-en finnes, returneres en 404-feil
        if (story == null)
            return NotFound("Historien ble ikke funnet.");

        // Sender historien videre til visningen (View),
        // der tittel, beskrivelse, vanskelighetsgrad og andre detaljer vises
        return View(story);
    }


    // Viser skjema for å opprette en ny historie
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Story());
    }

    // Mottar data fra skjemaet og oppretter en ny historie
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Story model)
    {
        // Sjekker at alle feltene i modellen er gyldige
        // (f.eks. at tittel og beskrivelse ikke er tomme)
        if (!ModelState.IsValid)
            return View(model); // Hvis noe er feil, vis skjemaet igjen med feilmeldinger

        // Kaller repository-metoden som legger historien inn i databasen
        // Hvis historien er privat, opprettes automatisk en unik tilgangskode (Code)
        await _repo.CreateStory(model);

        // Etter at historien er lagret, sendes brukeren tilbake til oversikten over historier
        return RedirectToAction(nameof(Index));
    }


    // Viser skjemaet for å redigere en eksisterende historie
    [HttpGet]
    // Henter en eksisterende historie og viser den i redigeringsskjemaet
    public async Task<IActionResult> Edit(int id)
    {
        var story = await _repo.GetStoryById(id);

        if (story == null)
            return NotFound("Historien ble ikke funnet.");
        return View(story);
    }

    // Mottar endringer fra skjemaet og oppdaterer historien
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Mottar data fra redigeringsskjemaet og oppdaterer en eksisterende historie i databasen
    public async Task<IActionResult> Edit(Story model)
    {
        // Sjekker at alle feltene i modellen er gyldige i henhold til datavalideringen (DataAnnotations)
        // Hvis valideringen feiler (f.eks. manglende tittel eller ugyldig verdi), vises skjemaet igjen med feilmeldinger
        if (!ModelState.IsValid)
            return View(model);

        // Kaller repository-metoden som oppdaterer historien i databasen
        // Repositoryet sørger også for at riktig kode settes automatisk hvis historien er privat
        await _repo.UpdateStory(model);

        // Etter at endringene er lagret, sendes brukeren videre til detaljsiden for historien
        return RedirectToAction(nameof(Details), new { id = model.StoryId });
    }

    // Sletter en historie
    // Sletter en historie fra databasen
    [HttpPost]
    [ValidateAntiForgeryToken] // Hindrer CSRF-angrep – sørger for at sletting kun skjer via et gyldig skjema i appen
    public async Task<IActionResult> Delete(int id)
    {
        var story = await _repo.GetStoryById(id);

        if (story == null)
            return NotFound("Historien ble ikke funnet.");

        // Kaller repository-metoden som faktisk sletter historien fra databasen
        var ok = await _repo.DeleteStory(id);

        // Sjekker at slettingen ble utført som forventet
        if (!ok)
            return BadRequest("Kunne ikke slette historien."); // Returnerer feilmelding hvis slettingen feilet

        // Hvis alt gikk bra, send brukeren tilbake til oversikten (Index)
        return RedirectToAction(nameof(Index));
    }
}
*/