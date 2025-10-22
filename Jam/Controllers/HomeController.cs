using Microsoft.AspNetCore.Mvc;
using Jam.DAL.StoryDAL;
using Jam.DAL.UserDAL;
using Jam.ViewModels;


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
                GameCode = s.Code ?? "",
                CardType = Jam.Models.Enums.GameCardType.BrowseMode
            }).ToList();

            ViewBag.FirstName = user?.Firstname ?? "Player";
            return View(model);           // ← VIKTIG: send inn modellen
        }
        
        


    }
}
