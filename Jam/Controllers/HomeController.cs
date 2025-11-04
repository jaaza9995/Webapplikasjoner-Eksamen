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


        public HomeController(IStoryRepository stories)
        {
            _stories = stories;
     
        }
        public async Task<IActionResult> Index()
        {
            int userId = 1; // midlertidig til du kobler til innlogging

            // Hent stories via DAL i stedet for direkte DbContext
            var yourStories = await _stories.GetStoriesByUserId(userId);

            // Hent nylig spilte (eksempel på gjenbruk av annen DAL-metode)
            var recentlyPlayed = await _stories.GetMostRecentPlayedStories(userId, 3);

            // Lag viewmodell for MyGames
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

            // Legg også til recently played (om du vil vise det i viewet)
            model.AddRange(recentlyPlayed.Select(s => new GameCardViewModel
            {
                Title = s.Title,
                Description = s.Description,
                DifficultyOptions = s.DifficultyLevel.ToString(),
                Accessible = s.Accessible,
                NumberOfQuestions = s.QuestionScenes?.Count ?? 0,
                GameCode = s.GameCode ?? "",
                CardType = GameCardType.RecentlyPlayed
            }));

            // Succes-melding fra TempData
            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View(model);
        }
    }
}