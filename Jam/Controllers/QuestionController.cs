using Microsoft.AspNetCore.Mvc;
using Jam.Models;
using Jam.DAL.QuestionDAL;

namespace Jam.Controllers;

// Angir grunnruten (URL-strukturen) for alle metoder i denne kontrolleren.
// Dette betyr at alle handlinger her tilhører en bestemt scene, og får URL-er som:
// /Scenes/{sceneId}/Questions/...  
// {sceneId:int} betyr at sceneId må være et heltall, og verdien sendes automatisk som parameter til metodene.
[Route("Scenes/{sceneId:int}/Questions")]
public class QuestionController : Controller
{
    // Felt som brukes til å få tilgang til spørsmål-data via repository-laget (DAL).
    // Repositoryet håndterer alle databaseoperasjoner slik at kontrolleren slipper å bruke DbContext direkte.
    private readonly IQuestionRepository _repo;

    // Konstruktør som mottar et IQuestionRepository-objekt gjennom avhengighetsinjeksjon (Dependency Injection).
    // Dette gjør det mulig å bytte ut implementasjonen (f.eks. for testing) uten å endre kontrolleren.
    public QuestionController(IQuestionRepository repo)
    {
        _repo = repo; // Lagre referansen slik at hele kontrolleren kan bruke den
    }

    // Viser en liste over alle spørsmål som hører til en bestemt scene.
    // Repositoryet har ikke en egen metode for å hente spørsmål per scene,
    // så vi henter alle spørsmål fra databasen og filtrerer dem i minnet basert på sceneId.
    [HttpGet("")]
    public async Task<IActionResult> Index(int sceneId)
    {
        // Henter alle spørsmål fra databasen via repositoryet
        var all = await _repo.GetAllQuestions();

        // Filtrerer listen slik at vi kun får spørsmål som tilhører riktig scene
        var list = all.Where(q => q.SceneId == sceneId).ToList();

        // Sender listen videre til visningen (View) for å vises på nettsiden
        return View(list);
    }


    // Viser skjemaet for å opprette et nytt spørsmål i en bestemt scene
    [HttpGet("Create")]
    public IActionResult Create(int sceneId)
    {
        // Oppretter et nytt Question-objekt der SceneId settes automatisk, slik at det nye spørsmålet kobles til riktig scene.
        var question = new Question { SceneId = sceneId };

        // Sender objektet til visningen (View) slik at feltene kan fylles ut
        return View(question);
    }

    // Mottar data fra skjemaet for å opprette et nytt spørsmål i en scene
    [HttpPost("Create")]
    [ValidateAntiForgeryToken] // Beskytter mot CSRF-angrep ved å validere en sikkerhetstoken
    public async Task<IActionResult> Create(int sceneId, Question model)
    {
        // Sjekker om alle feltene i modellen er gyldige i henhold til datavalideringen (DataAnnotations)
        if (!ModelState.IsValid)
        {
            // Hvis noe mangler eller er ugyldig, vises skjemaet på nytt med feilmeldinger
            return View(model);
        }

        // Sørger for at spørsmålet blir koblet til riktig scene før det lagres
        model.SceneId = sceneId;

        // Kaller repository-metoden som legger spørsmålet inn i databasen
        await _repo.CreateQuestion(model);

        // Sender brukeren tilbake til listen over spørsmål for denne scenen
        return RedirectToAction(nameof(Index), new { sceneId });
    }


    // Viser detaljene for ett bestemt spørsmål i en gitt scene
    // Eksempel på URL: /Scenes/1/Questions/5
    // Her er 1 = sceneId og 5 = questionId
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int sceneId, int id)
    {
        // Henter spørsmålet fra databasen via repositoryet basert på ID
        var q = await _repo.GetQuestionById(id);

        // Sjekker at spørsmålet finnes, og at det faktisk tilhører riktig scene
        if (q == null || q.SceneId != sceneId)
            return NotFound("Spørsmål ikke funnet.");

        // Sender spørsmålet videre til visningen slik at det kan vises for brukeren
        return View(q);
    }

    // Viser redigeringsskjemaet for et bestemt spørsmål i en scene
    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int sceneId, int id)
    {
        var q = await _repo.GetQuestionById(id);

        if (q == null || q.SceneId != sceneId)
            return NotFound("Spørsmål ikke funnet.");

        // Sender spørsmålet til visningen slik at feltene fylles ut i redigeringsskjemaet
        return View(q);
    }

    // Mottar oppdaterte data fra redigeringsskjemaet for et spørsmål
    // URL-eksempel: /Scenes/1/Questions/5/Edit
    [HttpPost("{id:int}/Edit")]
    [ValidateAntiForgeryToken] // Hindrer CSRF-angrep ved å kreve en gyldig sikkerhetstoken i POST-skjemaet
    public async Task<IActionResult> Edit(int sceneId, int id, Question model)
    {
        // Sjekker at ID-en i URL-en og i modellen stemmer overens
        if (id != model.QuestionId)
            return BadRequest("ID mismatch.");

        // Sjekker at spørsmålet tilhører riktig scene
        if (model.SceneId != sceneId)
            return BadRequest("Scene-ID mismatch.");

        // Sjekker at alle felt i modellen er gyldige (DataAnnotations)
        if (!ModelState.IsValid)
            return View(model);

        // Oppdaterer spørsmålet i databasen via repositoryet
        await _repo.UpdateQuestion(model);

        // Sender brukeren tilbake til detaljsiden for spørsmålet etter lagring
        return RedirectToAction(nameof(Details), new { sceneId, id = model.QuestionId });
    }



    // Sletter et spørsmål basert på ID
    // URL-eksempel: /Scenes/1/Questions/5/Delete
    [HttpPost("{id:int}/Delete")]
    [ValidateAntiForgeryToken] // Sikrer at sletting bare kan utføres fra et ekte skjema i appen
    public async Task<IActionResult> Delete(int sceneId, int id)
    {
        // Henter spørsmålet for å sjekke at det finnes og tilhører riktig scene
        var q = await _repo.GetQuestionById(id);
        if (q == null || q.SceneId != sceneId)
            return NotFound("Spørsmål ikke funnet.");

        // Kaller repositoryet for å slette spørsmålet fra databasen
        var ok = await _repo.DeleteQuestion(id);
        if (!ok)
            return BadRequest("Kunne ikke slette.");

        // Sender brukeren tilbake til spørsmål-listen for denne scenen etter sletting
        return RedirectToAction(nameof(Index), new { sceneId });
    }
}