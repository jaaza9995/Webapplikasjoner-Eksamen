using Microsoft.AspNetCore.Mvc;
using Jam.DAL.UserDAL;
using Jam.DAL.StoryDAL;

namespace Jam.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly IStoryRepository _stories;

        public UserController(IUserRepository users, IStoryRepository stories)
        {
            _users = users;
            _stories = stories;
        }

        // GET api/users/1  -> enkel brukerprofil
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById([FromRoute] int id)
        {
            var u = await _users.GetUserById(id);
            if (u == null) return NotFound(new { error = "Bruker ikke funnet." });

            return Ok(new
            {
                u.UserId,
                name = $"{u.Firstname} {u.Lastname}".Trim(),
                username = u.Username,
                email = u.Email
            });
        }

        // GET api/users/1/stats -> teller egne spill
        [HttpGet("{id:int}/stats")]
        public async Task<ActionResult<object>> GetStats([FromRoute] int id)
        {
            var u = await _users.GetUserById(id);
            if (u == null) return NotFound(new { error = "Bruker ikke funnet." });

            var mine = await _stories.GetStoriesByUserId(id);
            return Ok(new { userId = id, myGamesCount = mine.Count() });
        }

        // GET api/users/1/stories -> list opp brukerens stories (samme dataform som i /api/me/stories)
        [HttpGet("{id:int}/stories")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserStories([FromRoute] int id)
        {
            var list = await _stories.GetStoriesByUserId(id);
            var result = list.Select(s => new
            {
                storyId = s.StoryId,
                title = s.Title,
                description = s.Description,
                difficulty = s.DifficultyLevel.ToString(),
                editTargets = new
                {
                    story = $"/api/create/story/{s.StoryId}",
                    summary = $"/api/create/{s.StoryId}/summary"
                }
            });

            return Ok(result);
        }
    }
}
