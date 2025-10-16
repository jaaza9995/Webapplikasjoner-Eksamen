using Microsoft.AspNetCore.Mvc;
using Jam.DAL.SceneDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.ViewModels;
using Jam.Models.Enums;
//japp
namespace Jam.Controllers
{
    [ApiController]
    [Route("api/play")]
    public class PlayController : ControllerBase
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

        // POST api/play/start/public
        // Body: { "storyId": 1, "userId": 1 }
        // Oppretter ny økt på Level=3 og posisjonerer i Intro.
        [HttpPost("start/public")]
        public async Task<ActionResult<object>> StartPublic([FromBody] StartPublicRequest dto)
        {
            if (dto.StoryId <= 0) return BadRequest(new { error = "Ugyldig storyId." });

            var session = await _sessions.CreatePlayingSession(dto.UserId, dto.StoryId);
            return Ok(new
            {
                sessionId = session.PlayingSessionId
            });
        }

        // GET api/play/scene/{sessionId}
        // Returnerer aktuell scene som skal vises (Intro/Question/Ending).
        [HttpGet("scene/{sessionId:int}")]
        public async Task<ActionResult<object>> GetScene([FromRoute] int sessionId)
        {
            var session = await _sessions.GetPlayingSessionById(sessionId);
            if (session == null) return NotFound(new { error = "Økt ikke funnet." });
            if (session.CurrentSceneId == null)
                return Ok(new { type = "ended", score = session.Score, level = session.CurrentLevel });

            var scene = await _scenes.GetSceneWithDetailsById(session.CurrentSceneId.Value);
            if (scene == null) return NotFound(new { error = "Scene ikke funnet." });

            // Vis 4/3/2 alternativer basert på Level 3/2/1
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
                type = scene.SceneType.ToString(),   // Introduction | Question | EndingGood | ...
                level = session.CurrentLevel,
                score = session.Score,
                payload = vm
            });
        }

        // POST api/play/next
        // Body: { "sessionId": 1, "nextSceneId": 12 }
        // Brukes etter feedback for å flytte til neste scene.
        [HttpPost("next")]
        public async Task<ActionResult<object>> Next([FromBody] NextRequest dto)
        {
            var session = await _sessions.GetPlayingSessionById(dto.SessionId);
            if (session == null) return NotFound(new { error = "Økt ikke funnet." });

            if (dto.NextSceneId == null)
            {
                // Ingen neste scene => slutt
                await _sessions.FinishSession(dto.SessionId, session.Score, session.CurrentLevel);
                return Ok(new { ended = true, score = session.Score, level = session.CurrentLevel });
            }

            await _sessions.MoveToNextScene(dto.SessionId, dto.NextSceneId.Value);
            return Ok(new { moved = true });
        }

        // POST api/play/answer
        // Body: { "sessionId": 1, "answerId": 99 }
        // Regler:
        //  - Riktig: L3 +10, L2 +5, L1 +2. L2->L3, L1->L2.
        //  - Galt:   L3->L2, L2->L1, L1 -> Game Over.
        // Returnerer feedback-tekst (fra AnswerOption.SceneText) + neste scene-id for /next.
        [HttpPost("answer")]
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
                    // Game Over
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
                newLevel--; // 3->2, 2->1
            }

            var newScore = session.Score + add;

            // Ikke flytt til neste scene her; vi viser først feedback så klienten kaller /next
            // Bruk repo sin "AnswerQuestion" for å lagre ny score/level (men behold nåværende posisjon)
            // Triks: oppgi currentSceneId som nextSceneId for å ikke flytte i repo-metoden.
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
                // klienten viser feedback-overlay, og kaller deretter /api/play/next med denne verdien
                nextSceneId = answer.NextSceneId
            });
        }

        // POST api/play/finish
        // Body: { "sessionId": 1 }
        // Kan kalles ved manuell avslutning etter Ending-scene.
        [HttpPost("finish")]
        public async Task<ActionResult<object>> Finish([FromBody] FinishRequest dto)
        {
            var session = await _sessions.GetPlayingSessionById(dto.SessionId);
            if (session == null) return NotFound(new { error = "Økt ikke funnet." });

            await _sessions.FinishSession(dto.SessionId, session.Score, session.CurrentLevel);
            return Ok(new { ended = true, score = session.Score, level = session.CurrentLevel });
        }
    }

    // ---------- DTOs ----------
    public class StartPublicRequest
    {
        public int StoryId { get; set; }
        public int UserId { get; set; } = 1;
    }

    public class NextRequest
    {
        public int SessionId { get; set; }
        public int? NextSceneId { get; set; }
    }

    public class AnswerRequest
    {
        public int SessionId { get; set; }
        public int AnswerId { get; set; }
    }

    public class FinishRequest
    {
        public int SessionId { get; set; }
    }
}
