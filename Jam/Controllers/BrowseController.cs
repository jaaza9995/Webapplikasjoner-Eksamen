using Microsoft.AspNetCore.Mvc;
using Jam.DAL.StoryDAL;
using Jam.DAL.SceneDAL;
using Jam.ViewModels;
using Jam.Models.Enums;

namespace Jam.Controllers
{
    // Håndterer "Add new game": liste/søk av public spill + private kodeflyt
    [ApiController] // Gjør klassen til en API-kontroller (returnerer JSON)
    [Route("api/browse")] // Grunn-URL for alle endepunkter i denne kontrolleren
    public class BrowseController : ControllerBase // Arver fra ControllerBase (uten Views)
    {
        private readonly IStoryRepository _stories; // Håndterer databaseoperasjoner for historier
        private readonly ISceneRepository _scenes;  // Håndterer databaseoperasjoner for scener

        // Konstruktør som får inn repositories via dependency injection
        public BrowseController(IStoryRepository stories, ISceneRepository scenes)
        {
            _stories = stories; 
            _scenes  = scenes;
        }
    

        // GET api/browse/public?search=...
        // Returnerer kort-data for public stories (tittel, beskrivelse, antall spørsmål, vanskelighet)
        [HttpGet("public")]
        public async Task<ActionResult<PlayNewPublicGameViewModel>> GetPublic([FromQuery] string? search)
        {
            var all = await _stories.GetAllPublicStories();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(s => (s.Title ?? string.Empty)
                                .Contains(search, StringComparison.OrdinalIgnoreCase));

            // Antall spørsmål pr. story uten å endre DAL:
            // Hent scenene for hver story via ISceneRepository og tell Question-scener.
            var result = new PlayNewPublicGameViewModel
            {
                SearchQuery = search ?? string.Empty,
                Stories = await Task.WhenAll(all.Select(async s =>
                {
                    var storyScenes = await _scenes.GetScenesByStoryId(s.StoryId);
                    var questionCount = storyScenes.Count(sc => sc.SceneType == SceneType.Question);

                    return new StoryListViewModel
                    {
                        Id = s.StoryId,
                        Title = s.Title ?? "",
                        Introduction = s.Description ?? "",   // bruker Description som kort-tekst
                        QuestionCount = questionCount,
                        Difficulty = s.DifficultyLevel
                    };
                }))
            };

            return Ok(result);
        }

        // POST api/browse/private/validate
        // Body: { "code": "ABCD1234" }
        // Brukes av overlay for å vise "Is this the game you want to play?"
        [HttpPost("private/validate")]
        public async Task<ActionResult<object>> ValidatePrivate([FromBody] EnterCodeViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { error = "Kode mangler." });

            var story = await _stories.GetPrivateStoryByCode(model.Code);
            if (story == null) return NotFound(new { error = "Ugyldig kode." });

            // Oppsummering til overlay
            return Ok(new
            {
                storyId = story.StoryId,
                title = story.Title,
                description = story.Description,
                difficulty = story.DifficultyLevel.ToString()
            });
        }

        // POST api/browse/private/start
        // Body: { "code": "ABCD1234", "userId": 1 }
        // UI: Trykk "Yes" i overlay -> kall dette; du kan redirecte i UI til /Play/Scene?sessionId=...
        [HttpPost("private/start")]
        public async Task<ActionResult<object>> StartPrivate([FromBody] StartByCodeRequest dto,
                                                             [FromServices] Jam.DAL.PlayingSessionDAL.IPlayingSessionRepository sessions)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { error = "Kode mangler." });

            var story = await _stories.GetPrivateStoryByCode(dto.Code);
            if (story == null)
                return NotFound(new { error = "Ugyldig kode." });

            // Opprett ny spilløkt via eksisterende DAL
            // NB: repo-metoden lager start i Intro + setter MaxScore (spørsmålsantall*10) + Level=3
            var session = await sessions.CreatePlayingSession(dto.UserId, story.StoryId);

            // Frontend kan navigere til din eksisterende Play/Scene
            var nextUrl = Url.Action("Scene", "Play", new { sessionId = session.PlayingSessionId }) ?? $"/Play/Scene?sessionId={session.PlayingSessionId}";

            return Ok(new
            {
                sessionId = session.PlayingSessionId,
                storyId   = story.StoryId,
                redirect  = nextUrl
            });
        }

        // GET api/browse/public/{id}
        // Henter detaljer for ett public spill (for et ev. detaljpanel)
        [HttpGet("public/{id:int}")]
        public async Task<ActionResult<object>> GetPublicDetails([FromRoute] int id)
        {
            var story = await _stories.GetPublicStoryById(id);
            if (story == null) return NotFound(new { error = "Spill ikke funnet." });

            var storyScenes = await _scenes.GetScenesByStoryId(id);
            var questionCount = storyScenes.Count(sc => sc.SceneType == SceneType.Question);

            return Ok(new
            {
                storyId = story.StoryId,
                title = story.Title,
                description = story.Description,
                difficulty = story.DifficultyLevel.ToString(),
                questionCount
            });
        }
    }

    // Request-DTO for private start
    public class StartByCodeRequest
    {
        public string Code { get; set; } = string.Empty;
        public int UserId { get; set; } = 1; // placeholder til dere kobler på ekte auth
    }
}
