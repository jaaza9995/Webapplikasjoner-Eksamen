using Microsoft.AspNetCore.Mvc;
using Jam.DAL.SceneDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.ViewModels;
using Jam.Models.Enums;

namespace Jam.Controllers
{
    // ÉN kontroller som håndterer både Views og API
    // Viktig: IKKE [ApiController] på klassen når vi også skal returnere View()
    public class PlayController : Controller
    {
        private readonly ISceneRepository _scenes;
        private readonly IPlayingSessionRepository _sessions;
        private readonly IAnswerOptionRepository _answers;

        public PlayController(
            ISceneRepository scenes,
            IPlayingSessionRepository sessions,
            IAnswerOptionRepository answers)
        {
            _scenes = scenes;
            _sessions = sessions;
            _answers = answers;
        }

        // ---------- VIEWS ----------
        // GET /Play
        [HttpGet("/Play")]
        public IActionResult Index() => View("~/Views/Play/Index.cshtml");

        // GET /Play/Scene
        [HttpGet("/Play/Scene")]
        public IActionResult Scene() => View("~/Views/Play/Scene.cshtml");

        // ---------- API ----------
        // POST /api/play/start/public
        [HttpPost("/api/play/start/public")]
        public async Task<ActionResult<object>> StartPublic([FromBody] StartPublicRequest dto)
        {
            if (dto.StoryId <= 0) return BadRequest(new { error = "Ugyldig storyId." });

            var session = await _sessions.CreatePlayingSession(dto.UserId, dto.StoryId);
            return Ok(new { sessionId = session.PlayingSessionId });
        }

        // GET /api/play/scene/{sessionId}
        [HttpGet("/api/play/scene/{sessionId:int}")]
        public async Task<ActionResult<object>> GetScene([FromRoute] int sessionId)
        {
            var session = await _sessions.GetPlayingSessionById(sessionId);
            if (session == null) return NotFound(new { error = "Økt ikke funnet." });
            if (session.CurrentSceneId == null)
                return Ok(new { type = "ended", score = session.Score, level = session.CurrentLevel });

            var scene = await _scenes.GetSceneWithDetailsById(session.CurrentSceneId.Value);
            if (scene == null) return NotFound(new { error = "Scene ikke funnet." });

            var max = session.CurrentLevel == 3 ? 4 : session.CurrentLevel == 2 ? 3 : 2;
            var answers = scene.Question?.AnswerOptions?
                .OrderBy(a => a.AnswerOptionId)
                .Take(max)
                .Select(a => new AnswerOptionViewModel { AnswerId = a.AnswerOptionId, Text = a.Answer })
                .ToList() ?? new List<AnswerOptionViewModel>();

            var vm = new PlaySceneViewModel
            {
                SessionId = sessionId,
                SceneId = scene.SceneId,
                SceneText = scene.SceneText,
                SceneType = scene.SceneType.ToString(),
                Question = scene.Question?.QuestionText ?? string.Empty,
                NextSceneId = scene.NextSceneId,
                Answers = answers
            };

            return Ok(new
            {
                type = scene.SceneType.ToString(),
                level = session.CurrentLevel,
                score = session.Score,
                payload = vm
            });
        }

        // POST /api/play/next
        [HttpPost("/api/play/next")]
        public async Task<ActionResult<object>> Next([FromBody] NextRequest dto)
        {
            var session = await _sessions.GetPlayingSessionById(dto.SessionId);
            if (session == null) return NotFound(new { error = "Økt ikke funnet." });

            if (dto.NextSceneId == null)
            {
                await _sessions.FinishSession(dto.SessionId, session.Score, session.CurrentLevel);
                return Ok(new { ended = true, score = session.Score, level = session.CurrentLevel });
            }

            await _sessions.MoveToNextScene(dto.SessionId, dto.NextSceneId.Value);
            return Ok(new { moved = true });
        }

        // POST /api/play/answer
        [HttpPost("/api/play/answer")]
        public async Task<ActionResult<object>> Answer([FromBody] AnswerRequest dto)
        {
            var session = await _sessions.GetPlayingSessionById(dto.SessionId);
            if (session == null) return NotFound(new { error = "Økt ikke funnet." });

            var answer = await _answers.GetAnswerOptionById(dto.AnswerId);
            if (answer == null) return NotFound(new { error = "Svar ikke funnet." });

            bool correct = answer.IsCorrect;
            int add = 0;
            int newLevel = session.CurrentLevel;

            if (correct)
            {
                add = newLevel == 3 ? 10 : newLevel == 2 ? 5 : 2;
                if (newLevel == 2) newLevel = 3;
                else if (newLevel == 1) newLevel = 2;
            }
            else
            {
                if (newLevel == 1)
                {
                    await _sessions.FinishSession(dto.SessionId, session.Score, newLevel);
                    return Ok(new
                    {
                        ended = true,
                        gameOver = true,
                        score = session.Score,
                        level = newLevel,
                        feedback = string.IsNullOrWhiteSpace(answer.SceneText) ? "Game over." : answer.SceneText
                    });
                }
                newLevel--;
            }

            var newScore = session.Score + add;

            await _sessions.AnswerQuestion(dto.SessionId, session.CurrentSceneId ?? 0, newScore, newLevel);

            return Ok(new
            {
                ended = false,
                correct,
                addedPoints = add,
                score = newScore,
                level = newLevel,
                feedback = string.IsNullOrWhiteSpace(answer.SceneText)
                    ? (correct ? "Riktig!" : "Feil.")
                    : answer.SceneText,
                nextSceneId = answer.NextSceneId
            });
        }

        // POST /api/play/finish
        [HttpPost("/api/play/finish")]
        public async Task<ActionResult<object>> Finish([FromBody] FinishRequest dto)
        {
            var session = await _sessions.GetPlayingSessionById(dto.SessionId);
            if (session == null) return NotFound(new { error = "Økt ikke funnet." });

            await _sessions.FinishSession(dto.SessionId, session.Score, session.CurrentLevel);
            return Ok(new { ended = true, score = session.Score, level = session.CurrentLevel });
        }
    }

    // DTOs (uendret)
    public class StartPublicRequest { public int StoryId { get; set; } public int UserId { get; set; } = 1; }
    public class NextRequest { public int SessionId { get; set; } public int? NextSceneId { get; set; } }
    public class AnswerRequest { public int SessionId { get; set; } public int AnswerId { get; set; } }
    public class FinishRequest { public int SessionId { get; set; } }
}
