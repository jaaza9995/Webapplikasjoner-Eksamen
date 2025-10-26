using Microsoft.AspNetCore.Mvc;
using Jam.DAL.StoryDAL;
using Jam.DAL.UserDAL;
using Jam.ViewModels;
using Jam.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Jam.DAL;
using Microsoft.EntityFrameworkCore;


namespace Jam.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStoryRepository _stories;
        private readonly IUserRepository _users;
        private readonly StoryDbContext _db;

        public HomeController(IStoryRepository stories, IUserRepository users, StoryDbContext db)
        {
            _stories = stories;
            _users = users;
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var yourGames = await _db.Stories
            .Where(s => s.UserId == 1)         // bytt til innlogget bruker senere
            .Include(s => s.QuestionScenes)
            .AsNoTracking()
            .ToListAsync();
            
            int userId = 1;

            var user = await _users.GetUserById(userId);
            var yourStories = await _stories.GetStoriesByUserId(userId);

            var model = yourStories.Select(s => new GameCardViewModel
            {
                Title = s.Title,
                Description = s.Description,
                DifficultyOptions = s.DifficultyLevel.ToString(),
                Accessible = s.Accessible,
                NumberOfQuestions = s.QuestionScenes?.Count ?? 0,
                GameCode = s.GameCode ?? "",
                CardType = GameCardType.MyGames
            }).ToList();

            ViewBag.FirstName = user?.Firstname ?? "Player";
            return View(model);           // ← VIKTIG: send inn modellen
        }
        
    }
}
