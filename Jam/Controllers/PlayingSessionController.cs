using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Jam.DAL.StoryDAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.QuestionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.Models;
using Jam.Models.Enums;

namespace Jam.Controllers
{
    // Styrer spillflyten: start -> scene -> svar -> slutt
    public class PlayController : Controller
    {
        private readonly IStoryRepository _stories;
        private readonly ISceneRepository _scenes;
        private readonly IQuestionRepository _questions;
        private readonly IAnswerOptionRepository _answers;
        private readonly IPlayingSessionRepository _sessions;
        private readonly ILogger<PlayController> _logger;

        public PlayController(
            IStoryRepository stories,
            ISceneRepository scenes,
            IQuestionRepository questions,
            IAnswerOptionRepository answers,
            IPlayingSessionRepository sessions,
            ILogger<PlayController> logger)
        {
            _stories = stories;
            _scenes = scenes;
            _questions = questions;
            _answers = answers;
            _sessions = sessions;
            _logger = logger;
        }

        // --------------------------- START (OFFENTLIG) ---------------------------
        // Start et offentlig spill via storyId (f.eks. fra Browse-siden).
        [HttpPost]
        public async Task<IActionResult> StartPublic(int storyId, int userId = 1)
        {
            var story = await _stories.GetPublicStoryById(storyId);
            if (story == null) return BadRequest("Ugyldig storyId.");

            // Opprett ny spilløkt og bump 'Played/Dnf'
            var session = await _sessions.CreatePlayingSession(userId, storyId);
            await _stories.IncrementPlayed(storyId);

            // Gå til første scene
            return RedirectToAction(nameof(Scene), new { sessionId = session.PlayingSessionId });
        }

        // --------------------------- START (PRIVAT) ------------------------------
        // Start et privat spill via kode (f.eks. fra 'Enter code').
        [HttpPost]
        public async Task<IActionResult> StartByCode(string code, int userId = 1)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest("Kode mangler.");

            var story = await _stories.GetPrivateStoryByCode(code);
            if (story == null) return BadRequest("Ugyldig kode.");

            var session = await _sessions.CreatePlayingSession(userId, story.StoryId);
            await _stories.IncrementPlayed(story.StoryId);

            return RedirectToAction(nameof(Scene), new { sessionId = session.PlayingSessionId });
        }

        // --------------------------- VISE SCENE ---------------------------------
        // Viser gjeldende scene i økten (Intro eller Question).
        [HttpGet]
        public async Task<IActionResult> Scene(int sessionId)
        {
            var session = await _sessions.GetPlayingSessionById(sessionId);
            if (session == null || session.CurrentSceneId == null) return NotFound("Økt/scene ikke funnet.");

            // Hent scene inkl. Question + AnswerOptions
            var scene = await _scenes.GetSceneWithDetailsById(session.CurrentSceneId.Value);
            if (scene == null) return NotFound("Scene ikke funnet.");

            // Når SceneType == Question -> PlayScene.cshtml viser Question + AnswerOptions
            // Når SceneType == Introduction -> samme view kan vise kun SceneText + 'Neste'
            // (Du har PlayScene.cshtml og StartStory.cshtml; begge kan peke hit hvis ønskelig)
            var vm = new ViewModels.PlaySceneViewModel
            {
                SessionId = sessionId,
                SceneId = scene.SceneId,
                SceneType = scene.SceneType.ToString(),
                SceneText = scene.SceneText,
                Question = scene.Question?.QuestionText ?? string.Empty,
                Answers = scene.Question?.AnswerOptions?.Select(ao => new ViewModels.PlaySceneViewModel.AnswerVM
                {
                    AnswerOptionId = ao.AnswerOptionId,
                    Text = ao.Answer
                }).ToList() ?? new()
            };

            return View("PlayScene", vm);
        }

        // --------------------------- SVAR PÅ SPØRSMÅL ---------------------------
        // Tar imot valgt svar, oppdaterer score/level, og går videre til neste scene eller slutt.
        [HttpPost]
        public async Task<IActionResult> Answer(int sessionId, int answerOptionId)
        {
            var session = await _sessions.GetPlayingSessionById(sessionId);
            if (session == null || session.CurrentSceneId == null) return NotFound("Økt/scene ikke funnet.");

            // Finn valgt svar + nåværende scene & neste scene
            var chosen = await _answers.GetAnswerOptionById(answerOptionId);
            if (chosen == null) return BadRequest("Ugyldig svarvalg.");

            var currentScene = await _scenes.GetSceneById(session.CurrentSceneId.Value);
            if (currentScene == null) return NotFound("Scene ikke funnet.");

            // Poeng: +10 ved korrekt, ellers 0 (enkelt og tydelig)
            var add = chosen.IsCorrect ? 10 : 0;
            var newScore = session.Score + add;

            // Level: enkel indikator (du kan skru det av/på senere)
            var newLevel = session.CurrentLevel;

            // Neste scene (Question -> Question), eller null hvis slutt på spørsmål
            var next = await _scenes.GetNextScene(currentScene.SceneId);

            if (next == null)
            {
                // Ingen flere Question-scener -> finn riktig ending for brukerens score
                var ending = await _scenes.GetEndingSceneForStory(session.StoryId, session.UserId ?? 0);
                if (ending == null)
                {
                    // Fallback hvis ending mangler: avslutt total score og send til FinishStory
                    await _sessions.FinishSession(sessionId, newScore, newLevel);
                    return RedirectToAction(nameof(End), new { sessionId });
                }

                // Gå til ending-scenen
                await _sessions.AnswerQuestion(sessionId, ending.SceneId, newScore, newLevel);
                return RedirectToAction(nameof(End), new { sessionId });
            }

            // Fortsett til neste Question-scene
            await _sessions.AnswerQuestion(sessionId, next.SceneId, newScore, newLevel);
            return RedirectToAction(nameof(Scene), new { sessionId });
        }

        // --------------------------- SLUTT --------------------------------------
        // Viser slutt/ending. Oppdaterer også Story-statistikk.
        [HttpGet]
        public async Task<IActionResult> End(int sessionId)
        {
            var session = await _sessions.GetPlayingSessionById(sessionId);
            if (session == null) return NotFound("Økt ikke funnet.");

            // Hent ending basert på score%
            var ending = await _scenes.GetEndingSceneForStory(session.StoryId, session.UserId ?? 0);
            if (ending == null)
            {
                // Ingen ending definert -> avslutt likevel
                await _sessions.FinishSession(sessionId, session.Score, session.CurrentLevel);
                return View("FinishStory", session);
            }

            // Oppdater story-statistikk basert på ending-type
            if (ending.SceneType == SceneType.EndingGood)
                await _stories.IncrementFinished(session.StoryId);
            else if (ending.SceneType == SceneType.EndingBad)
                await _stories.IncrementFailed(session.StoryId);
            else
                await _stories.IncrementFinished(session.StoryId); // nøytral teller som gjennomført

            // Sett slutt-tid og lås økten
            await _sessions.FinishSession(sessionId, session.Score, session.CurrentLevel);

            // Du har FinishStory.cshtml – den kan få alt den trenger via session + ending-tekst
            ViewBag.EndingText = ending.SceneText;
            return View("FinishStory", session);
        }
    }
}

