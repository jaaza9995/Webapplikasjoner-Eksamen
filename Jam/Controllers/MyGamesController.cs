/*using Microsoft.AspNetCore.Mvc;
using Jam.DAL.UserDAL;
using Jam.DAL.StoryDAL;
using Jam.Models.Enums;

namespace Jam.Controllers
{
    [ApiController]
    [Route("api/me")]
    public class MyGamesController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly IStoryRepository _stories;

        public MyGamesController(IUserRepository users, IStoryRepository stories)
        {
            _users = users;
            _stories = stories;
        }

        // GET api/me/home?userId=1
        // Bruk til Home-siden: "Welcome, {username}" + antall spill
        [HttpGet("home")]
        public async Task<ActionResult<object>> GetHome([FromQuery] int userId)
        {
            var u = await _users.GetUserById(userId);
            if (u == null) return NotFound(new { error = "Bruker ikke funnet." });

            var mine = await _stories.GetStoriesByUserId(userId);
            return Ok(new
            {
                user = new { u.UserId, name = $"{u.Firstname} {u.Lastname}".Trim(), username = u.Username },
                stats = new { myGamesCount = mine.Count() }
            });
        }

        // GET api/me/stories?userId=1
        // Liste til seksjonen "My games" på Home
        [HttpGet("stories")]
        public async Task<ActionResult<object>> GetMyStories([FromQuery] int userId)
        {
            var list = await _stories.GetStoriesByUserId(userId);
            var result = list.Select(s => new
            {
                storyId = s.StoryId,
                title = s.Title,
                description = s.Description,
                difficulty = s.DifficultyLevel.ToString(),
                // UI: bruk denne til å åpne editor-wizarden (CreateGameController Upsert/GET-endepunkter)
                editTargets = new {
                    story = $"/api/create/story/{s.StoryId}",            // GET for å fylle steg 1
                    summary = $"/api/create/{s.StoryId}/summary",        // for oversikt
                    // resten (intro/questions/endings) slås opp dynamisk av editoren din
                }
            });

            return Ok(result);
        }

        // GET api/me/stories/{storyId}/can-edit?userId=1
        // (valgfritt) Sjekk eierskap før du viser Edit-knappen
        [HttpGet("stories/{storyId:int}/can-edit")]
        public async Task<ActionResult<object>> CanEdit([FromRoute] int storyId, [FromQuery] int userId)
        {
            var s = await _stories.GetStoryById(storyId);
            if (s == null) return NotFound(new { error = "Story ikke funnet." });

            var can = s.UserId == userId;
            return Ok(new { canEdit = can });
        }
    }
}
*/
