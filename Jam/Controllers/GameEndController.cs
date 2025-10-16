using Microsoft.AspNetCore.Mvc;
//japp

namespace Jam.Controllers
{
    public class GameEndController : Controller
    {
        [HttpGet]
        public IActionResult Index(int score, string endingType, string endingText)
        {
            ViewData["Score"] = score;
            ViewData["EndingType"] = endingType;
            ViewData["EndingText"] = endingText;
            return View();
        }
    }
}
