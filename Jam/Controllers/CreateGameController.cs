using Microsoft.AspNetCore.Mvc;
using Jam.DAL.StoryDAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.QuestionDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.Models;
using Jam.Models.Enums;
//japp

namespace Jam.Controllers
{
    [ApiController]
    [Route("api/create")]
    public class CreateGameController : ControllerBase
    {
        private readonly IStoryRepository _stories;
        private readonly ISceneRepository _scenes;
        private readonly IQuestionRepository _questions;
        private readonly IAnswerOptionRepository _answers;

        public CreateGameController(
            IStoryRepository stories,
            ISceneRepository scenes,
            IQuestionRepository questions,
            IAnswerOptionRepository answers)
        {
            _stories = stories;
            _scenes = scenes;
            _questions = questions;
            _answers = answers;
        }

        // =========================
        // A) STORY: UPSERT + GET
        // =========================

        // POST/PUT api/create/story (Upsert ved “Next” i steg 1)
        [HttpPost("story")]
        [HttpPut("story")]
        public async Task<ActionResult<object>> UpsertStory([FromBody] UpsertStoryRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { error = "Tittel mangler." });

            Story? s = null;
            if (dto.StoryId.HasValue)
            {
                s = await _stories.GetStoryById(dto.StoryId.Value);
                if (s == null) return NotFound(new { error = "Story ikke funnet." });

                s.Title = dto.Title.Trim();
                s.Description = dto.Description?.Trim() ?? string.Empty;
                s.DifficultyLevel = dto.Difficulty;
                s.Accessible = dto.Accessible;
                s.UserId = dto.UserId;

                await _stories.UpdateStory(s);
            }
            else
            {
                s = new Story
                {
                    Title = dto.Title.Trim(),
                    Description = dto.Description?.Trim() ?? string.Empty,
                    DifficultyLevel = dto.Difficulty,
                    Accessible = dto.Accessible,
                    UserId = dto.UserId
                };
                await _stories.CreateStory(s);
            }

            return Ok(new { storyId = s.StoryId });
        }

        // GET api/create/story/{storyId} (for “Back”/rediger)
        [HttpGet("story/{storyId:int}")]
        public async Task<ActionResult<object>> GetStory([FromRoute] int storyId)
        {
            var s = await _stories.GetStoryById(storyId);
            if (s == null) return NotFound(new { error = "Story ikke funnet." });

            return Ok(new
            {
                storyId = s.StoryId,
                s.Title,
                s.Description,
                Difficulty = s.DifficultyLevel.ToString(),
                Accessibility = s.Accessible.ToString()
            });
        }

        // =========================
        // B) INTRO: UPSERT + GET
        // =========================

        // POST/PUT api/create/{storyId}/intro (Upsert ved “Next” i intro-steg)
        [HttpPost("{storyId:int}/intro")]
        [HttpPut("{storyId:int}/intro")]
        public async Task<ActionResult<object>> UpsertIntro([FromRoute] int storyId, [FromBody] UpsertIntroRequest dto)
        {
            Scene? intro = null;

            if (dto.SceneId.HasValue)
            {
                intro = await _scenes.GetSceneById(dto.SceneId.Value);
                if (intro == null) return NotFound(new { error = "Intro-scene ikke funnet." });
                if (intro.SceneType != SceneType.Introduction) return BadRequest(new { error = "Scene er ikke Introduction." });

                intro.SceneText = dto.SceneText?.Trim() ?? string.Empty;
                await _scenes.UpdateScene(intro);
            }
            else
            {
                intro = new Scene
                {
                    StoryId = storyId,
                    SceneType = SceneType.Introduction,
                    SceneText = dto.SceneText?.Trim() ?? string.Empty
                };
                await _scenes.CreateScene(intro, previousSceneId: null);
            }

            return Ok(new { sceneId = intro.SceneId });
        }

        // GET api/create/{storyId}/intro (for “Back”/rediger)
        [HttpGet("{storyId:int}/intro")]
        public async Task<ActionResult<object>> GetIntro([FromRoute] int storyId)
        {
            var intro = await _scenes.GetIntroSceneByStoryId(storyId);
            if (intro == null) return NotFound(new { error = "Intro finnes ikke." });

            return Ok(new { sceneId = intro.SceneId, intro.SceneText });
        }

        // =======================================
        // C) QUESTION-SCENE: UPSERT + GET
        // =======================================

        // POST/PUT api/create/{storyId}/question (Upsert ved “Next” per spørsmål)
        [HttpPost("{storyId:int}/question")]
        [HttpPut("{storyId:int}/question")]
        public async Task<ActionResult<object>> UpsertQuestion(
            [FromRoute] int storyId,
            [FromBody] UpsertQuestionRequest dto)
        {
            if (dto.Answers == null || dto.Answers.Count != 4)
                return BadRequest(new { error = "Akkurat 4 svaralternativer kreves." });

            Scene? qScene;
            Question? q;

            if (dto.SceneId.HasValue)
            {
                // Update
                qScene = await _scenes.GetSceneById(dto.SceneId.Value);
                if (qScene == null) return NotFound(new { error = "Question-scene ikke funnet." });
                if (qScene.SceneType != SceneType.Question) return BadRequest(new { error = "Scene er ikke Question." });

                qScene.SceneText = dto.SceneContext?.Trim() ?? string.Empty;
                await _scenes.UpdateScene(qScene);

                q = await _questions.GetQuestionBySceneId(qScene.SceneId);
                if (q == null) return BadRequest(new { error = "Question mangler for scenen." });
                q.QuestionText = dto.QuestionText?.Trim() ?? string.Empty;
                await _questions.UpdateQuestion(q);

                // Oppdater/Upsert svar
                var existing = (await _answers.GetAnswerOptionsByQuestionId(q.QuestionId)).ToList();
                for (int i = 0; i < 4; i++)
                {
                    var incoming = dto.Answers[i];
                    if (i < existing.Count)
                    {
                        var upd = existing[i];
                        upd.Answer = incoming.Text?.Trim() ?? string.Empty;
                        upd.SceneText = incoming.Feedback?.Trim() ?? string.Empty;
                        upd.IsCorrect = incoming.IsCorrect;
                        await _answers.UpdateAnswerOption(upd);
                    }
                    else
                    {
                        var add = new AnswerOption
                        {
                            QuestionId = q.QuestionId,
                            Answer = incoming.Text?.Trim() ?? string.Empty,
                            SceneText = incoming.Feedback?.Trim() ?? string.Empty,
                            IsCorrect = incoming.IsCorrect
                        };
                        await _answers.CreateAnswerOption(add);
                    }
                }
            }
            else
            {
                // Create
                qScene = new Scene
                {
                    StoryId = storyId,
                    SceneType = SceneType.Question,
                    SceneText = dto.SceneContext?.Trim() ?? string.Empty
                };
                await _scenes.CreateScene(qScene, previousSceneId: dto.PreviousSceneId);

                q = new Question
                {
                    SceneId = qScene.SceneId,
                    QuestionText = dto.QuestionText?.Trim() ?? string.Empty
                };
                await _questions.CreateQuestion(q);

                foreach (var a in dto.Answers)
                {
                    var opt = new AnswerOption
                    {
                        QuestionId = q.QuestionId,
                        Answer = a.Text?.Trim() ?? string.Empty,
                        SceneText = a.Feedback?.Trim() ?? string.Empty,
                        IsCorrect = a.IsCorrect
                    };
                    await _answers.CreateAnswerOption(opt);
                }

                // Koble forrige scene → denne (og svarene i forrige scene → denne)
                if (dto.PreviousSceneId.HasValue)
                {
                    var prevQ = await _questions.GetQuestionBySceneId(dto.PreviousSceneId.Value);
                    if (prevQ != null)
                    {
                        var prevAns = await _answers.GetAnswerOptionsByQuestionId(prevQ.QuestionId);
                        foreach (var pa in prevAns)
                        {
                            pa.NextSceneId = qScene.SceneId;
                            await _answers.UpdateAnswerOption(pa);
                        }
                    }
                }
            }

            return Ok(new { sceneId = qScene.SceneId, questionId = q.QuestionId });
        }

        // GET api/create/question/{sceneId} (for “Back”/rediger)
        [HttpGet("question/{sceneId:int}")]
        public async Task<ActionResult<object>> GetQuestion([FromRoute] int sceneId)
        {
            var scene = await _scenes.GetSceneWithDetailsById(sceneId);
            if (scene == null || scene.SceneType != SceneType.Question)
                return NotFound(new { error = "Question-scene ikke funnet." });

            var question = scene.Question!;
            var opts = question.AnswerOptions.OrderBy(a => a.AnswerOptionId).ToList();

            return Ok(new
            {
                sceneId = scene.SceneId,
                sceneContext = scene.SceneText,
                questionId = question.QuestionId,
                questionText = question.QuestionText,
                answers = opts.Select(a => new
                {
                    answerId = a.AnswerOptionId,
                    text = a.Answer,
                    isCorrect = a.IsCorrect,
                    feedback = a.SceneText
                })
            });
        }

        // ===================================
        // D) ENDING: UPSERT + GET
        // ===================================

        // POST/PUT api/create/{storyId}/ending (Upsert på outro-siden; “Back” kan redigere)
        [HttpPost("{storyId:int}/ending")]
        [HttpPut("{storyId:int}/ending")]
        public async Task<ActionResult<object>> UpsertEnding(
            [FromRoute] int storyId,
            [FromBody] UpsertEndingRequest dto)
        {
            Scene? ending;

            if (dto.SceneId.HasValue)
            {
                ending = await _scenes.GetSceneById(dto.SceneId.Value);
                if (ending == null) return NotFound(new { error = "Ending-scene ikke funnet." });
                if (ending.SceneType != dto.EndingType) return BadRequest(new { error = "Ending-type samsvarer ikke med scene." });

                ending.SceneText = dto.SceneText?.Trim() ?? string.Empty;
                await _scenes.UpdateScene(ending);
            }
            else
            {
                ending = new Scene
                {
                    StoryId = storyId,
                    SceneType = dto.EndingType,
                    SceneText = dto.SceneText?.Trim() ?? string.Empty
                };
                await _scenes.CreateScene(ending, previousSceneId: dto.PreviousSceneId);

                // Koble forrige question-svar til denne endingen (hvis gitt)
                if (dto.PreviousSceneId.HasValue)
                {
                    var prevQ = await _questions.GetQuestionBySceneId(dto.PreviousSceneId.Value);
                    if (prevQ != null)
                    {
                        var prevAns = await _answers.GetAnswerOptionsByQuestionId(prevQ.QuestionId);
                        foreach (var pa in prevAns)
                        {
                            pa.NextSceneId = ending.SceneId;
                            await _answers.UpdateAnswerOption(pa);
                        }
                    }
                }
            }

            return Ok(new { sceneId = ending.SceneId, type = ending.SceneType.ToString() });
        }

        // GET api/create/ending/{sceneId}
        [HttpGet("ending/{sceneId:int}")]
        public async Task<ActionResult<object>> GetEnding([FromRoute] int sceneId)
        {
            var s = await _scenes.GetSceneById(sceneId);
            if (s == null || (s.SceneType != SceneType.EndingGood &&
                              s.SceneType != SceneType.EndingNeutral &&
                              s.SceneType != SceneType.EndingBad))
                return NotFound(new { error = "Ending-scene ikke funnet." });

            return Ok(new { sceneId = s.SceneId, type = s.SceneType.ToString(), s.SceneText });
        }

        // =========================
        // E) SUMMARY + SUBMIT
        // =========================

        // GET api/create/{storyId}/summary (for oversikt før submit)
        [HttpGet("{storyId:int}/summary")]
        public async Task<ActionResult<object>> Summary([FromRoute] int storyId)
        {
            var ordered = await _scenes.GetScenesInOrderByStoryId(storyId);
            var questions = ordered.Count(s => s.SceneType == SceneType.Question);
            var endings = ordered.Where(s => s.SceneType == SceneType.EndingGood ||
                                             s.SceneType == SceneType.EndingNeutral ||
                                             s.SceneType == SceneType.EndingBad)
                                 .Select(s => new { s.SceneId, type = s.SceneType.ToString() });

            return Ok(new
            {
                storyId,
                sceneCount = ordered.Count(),
                questionCount = questions,
                endings
            });
        }

        // POST api/create/{storyId}/submit (minst 3 spørsmål)
        [HttpPost("{storyId:int}/submit")]
        public async Task<ActionResult<object>> Submit([FromRoute] int storyId)
        {
            var qScenes = await _scenes.GetQuestionScenesInOrderByStoryId(storyId);
            if (qScenes.Count() < 3)
                return BadRequest(new { error = "Du må ha minst 3 spørsmål før du kan fullføre." });

            var details = await _stories.GetStoryById(storyId);
            return Ok(new { storyId, title = details?.Title, status = "submitted" });
        }

        // DELETE api/create/story/{storyId}
[HttpDelete("story/{storyId:int}")]
public async Task<ActionResult<object>> DeleteStory([FromRoute] int storyId)
{
    var story = await _stories.GetStoryById(storyId);
    if (story == null) return NotFound(new { error = "Story ikke funnet." });

    var ok = await _stories.DeleteStory(storyId);
    if (!ok) return BadRequest(new { error = "Kunne ikke slette story." });

    return Ok(new { deleted = true, storyId });
}

// DELETE api/create/scene/{sceneId}?previousSceneId=5
[HttpDelete("scene/{sceneId:int}")]
public async Task<ActionResult<object>> DeleteScene([FromRoute] int sceneId, [FromQuery] int? previousSceneId)
{
    var scene = await _scenes.GetSceneById(sceneId);
    if (scene == null) return NotFound(new { error = "Scene ikke funnet." });

    var ok = await _scenes.DeleteScene(sceneId, previousSceneId);
    if (!ok) return BadRequest(new { error = "Kunne ikke slette scene." });

    return Ok(new { deleted = true, sceneId });
}

// DELETE api/create/question/{questionId}
[HttpDelete("question/{questionId:int}")]
public async Task<ActionResult<object>> DeleteQuestion([FromRoute] int questionId)
{
    var q = await _questions.GetQuestionById(questionId);
    if (q == null) return NotFound(new { error = "Question ikke funnet." });

    var ok = await _questions.DeleteQuestion(questionId);
    if (!ok) return BadRequest(new { error = "Kunne ikke slette question." });

    return Ok(new { deleted = true, questionId });
}

// DELETE api/create/answer/{answerId}
[HttpDelete("answer/{answerId:int}")]
public async Task<ActionResult<object>> DeleteAnswer([FromRoute] int answerId)
{
    var a = await _answers.GetAnswerOptionById(answerId);
    if (a == null) return NotFound(new { error = "AnswerOption ikke funnet." });

    var ok = await _answers.DeleteAnswerOption(answerId);
    if (!ok) return BadRequest(new { error = "Kunne ikke slette answeroption." });

    return Ok(new { deleted = true, answerId });
}
    }

    // ============= DTOs =============

    public class UpsertStoryRequest
    {
        public int? StoryId { get; set; }           // null => create, ellers update
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;
        public Accessibility Accessible { get; set; } = Accessibility.Public;
        public int? UserId { get; set; }
    }

    public class UpsertIntroRequest
    {
        public int? SceneId { get; set; }           // null => create, ellers update
        public string SceneText { get; set; } = string.Empty;
    }

    public class UpsertQuestionRequest
    {
        public int? SceneId { get; set; }           // null => create, ellers update
        public int? PreviousSceneId { get; set; }   // kobling ved create
        public string SceneContext { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public List<CreateAnswerInput> Answers { get; set; } = new(); // alltid 4
    }

    public class CreateAnswerInput
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Feedback { get; set; }
    }

    public class UpsertEndingRequest
    {
        public int? SceneId { get; set; }           // null => create, ellers update
        public int? PreviousSceneId { get; set; }   // kobling ved create
        public SceneType EndingType { get; set; } = SceneType.EndingGood;
        public string SceneText { get; set; } = string.Empty;
    }
}
