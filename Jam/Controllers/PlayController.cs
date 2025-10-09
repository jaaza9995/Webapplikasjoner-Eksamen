/*using Microsoft.AspNetCore.Mvc;      
using Microsoft.EntityFrameworkCore;                 

namespace Jam.Controllers
{
    // PlayController styrer selve spillflyten for brukeren – hvordan spillet starter, går mellom scener og avsluttes.
    public class PlayController : Controller
    {
        private readonly AppDbContext _db; // Felt som gir tilgang til databasen via Entity Framework

        public PlayController(AppDbContext db) // Konstruktør 
        {
            _db = db;
        }

        // GET: /Play
        public IActionResult Index()
        {
            return View();
        }
        // Viser startsiden for Play (Views/Play/Index.cshtml) – en enkel side der brukeren kan skrive inn spillkoden (Story.Code)

        // POST: /Play/StartByCode
        [HttpPost]
        public async Task<IActionResult> StartByCode(string code, int userId = 1)
        {
            // 1. Finn historien (Story) som matcher koden brukeren skrev inn
            var story = await _db.Stories.FirstOrDefaultAsync(s => s.Code == code);
            if (story == null) return BadRequest("Ugyldig kode."); // Returner feilmelding hvis koden ikke finnes

            // 2. Finn første scene i historien (lavest SceneId eller markert som start)
            var firstScene = await _db.Scenes
                .Where(x => x.StoryId == story.StoryId)
                .OrderBy(x => x.SceneId) // Velger den første scenen i rekkefølgen
                .FirstOrDefaultAsync();

            if (firstScene == null) return BadRequest("Historien mangler scener.");

            // 3. Opprett en ny GameSession (lagrer fremdrift for spilleren)
            var session = new GameSession
            {
                StoryId = story.StoryId,
                UserId = userId,
                CurrentSceneId = firstScene.SceneId,
                Score = 0 // starter med 0 poeng
            };

            // 4. Legg sesjonen inn i databasen
            _db.GameSessions.Add(session);
            await _db.SaveChangesAsync(); // lagrer i databasen

            // 5. Send brukeren videre til første scene i spillet
            return RedirectToAction(nameof(Scene), new { sessionId = session.GameSessionId });
        }

        // GET: /Play/Scene?sessionId=123
        [HttpGet]
        public async Task<IActionResult> Scene(int sessionId)
        {
            // 1. Hent spilløkten fra databasen
            var session = await _db.GameSessions.FindAsync(sessionId);
            if (session == null) return NotFound(); // hvis ugyldig id, returner 404

            // 2. Hent scenen som spilleren er på akkurat nå
            var scene = await _db.Scenes
                .Include(s => s.Story) // inkluder historien scenen tilhører
                .FirstOrDefaultAsync(s => s.SceneId == session.CurrentSceneId);
            if (scene == null) return NotFound("Scene ikke funnet.");

            // 3. Hent eventuelt spørsmål som er koblet til denne scenen
            var question = await _db.Questions.FirstOrDefaultAsync(q => q.SceneId == scene.SceneId);

            // 4. Legg spørsmålet i ViewBag slik at visningen kan få tilgang
            ViewBag.Question = question;

            // 5. Send Scene-objektet til viewet (Views/Play/Scene.cshtml)
            return View(scene);
        }

        // POST: /Play/Answer
        [HttpPost]
        public async Task<IActionResult> Answer(int sessionId, int sceneId, string answer)
        {
            // 1. Hent spillerens nåværende økt (GameSession)
            var session = await _db.GameSessions.FindAsync(sessionId);
            if (session == null) return NotFound();

            // 2. Finn spørsmålet som hører til denne scenen
            var question = await _db.Questions.FirstOrDefaultAsync(q => q.SceneId == sceneId);
            if (question != null)
            {
                // 3. Sjekk om svaret er riktig (case-insensitive)
                var correct = string.Equals(answer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                if (correct) session.Score += 1; // +1 poeng ved riktig svar

                // (Valgfritt) Bruk AnswerOption-tabellen om du har flere mulige svar
                // var options = await _db.AnswerOptions.FirstOrDefaultAsync(o => o.QuestionID == question.QuestionId);
            }

            // 4. Finn neste scene
            var scene = await _db.Scenes.FirstOrDefaultAsync(s => s.SceneId == sceneId);
            if (scene?.NextSceneId == null)
            {
                // Hvis ingen neste scene – spill ferdig
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(End), new { sessionId });
            }

            // 5. Flytt spilleren videre til neste scene
            session.CurrentSceneId = scene.NextSceneId.Value;
            await _db.SaveChangesAsync();

            // 6. Last neste scene
            return RedirectToAction(nameof(Scene), new { sessionId });
        }

        // POST: /Play/Next  (brukes på scener uten spørsmål, kun “Neste”-knapp)
        [HttpPost]
        public async Task<IActionResult> Next(int sessionId)
        {
            // 1. Hent spilløkten
            var session = await _db.GameSessions.FindAsync(sessionId);
            if (session == null) return NotFound();

            // 2. Finn nåværende scene
            var scene = await _db.Scenes.FirstOrDefaultAsync(s => s.SceneId == session.CurrentSceneId);
            if (scene?.NextSceneId == null)
                return RedirectToAction(nameof(End), new { sessionId }); // Ferdig hvis ingen flere scener

            // 3. Oppdater spillerens posisjon i historien
            session.CurrentSceneId = scene.NextSceneId.Value;
            await _db.SaveChangesAsync();

            // 4. Send spilleren til neste scene
            return RedirectToAction(nameof(Scene), new { sessionId });
        }

        // GET: /Play/End  (spillets slutt)
        [HttpGet]
        public async Task<IActionResult> End(int sessionId)
        {
            // 1. Hent spilløkten og historien den tilhører
            var session = await _db.GameSessions
                .Include(s => s.Story)
                .FirstOrDefaultAsync(s => s.GameSessionId == sessionId);

            if (session == null) return NotFound();

            // 2. Send GameSession som modell til sluttvisningen (viser score, tittel osv.)
            return View(session); // Views/Play/End.cshtml
        }
    }
}
*/