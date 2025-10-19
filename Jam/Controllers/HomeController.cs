using Microsoft.AspNetCore.Mvc;
using Jam.DAL.StoryDAL;
using Jam.DAL.UserDAL;
using Jam.Models;

namespace Jam.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStoryRepository _stories;
        private readonly IUserRepository _users;

        public HomeController(IStoryRepository stories, IUserRepository users)
        {
            _stories = stories;
            _users = users;
        }

        public async Task<IActionResult> Index()
        {
            int userId = 1; // midlertidig "innlogget" bruker (Barry Allen i DBInit)

            var user = await _users.GetUserById(userId);
            var yourGames = await _stories.GetStoriesByUserId(userId);
            var recentlyPlayed = await _stories.GetMostRecentPlayedStories(userId, 5);

            ViewBag.FirstName = user?.Firstname ?? "Player";
            ViewBag.YourGames = yourGames;
            ViewBag.RecentlyPlayed = recentlyPlayed;

            return View();
        }
    }
}
