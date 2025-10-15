using Jam.DAL.StoryDAL;
using Jam.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Jam.Controllers
{
    // HomeController håndterer hovedsiden (forsiden) i applikasjonen og generelle visninger som ikke er knyttet til selve spillet.
    public class HomeController : Controller
    {
        private readonly IStoryRepository _stories; // Gir tilgang til historiene i databasen

        // Konstruktør –> kobler sammen kontrolleren og repositoryet
        public HomeController(IStoryRepository stories)
        {
            _stories = stories;
        }

        // Viser startsiden (Views/Home/Index.cshtml)
        public IActionResult Index()
        {
            return View();
        }

        // Viser skjema for å skrive inn spillkode
        [HttpGet]
        public IActionResult AddNewGame()
        {
            return View(new EnterCodeViewModel());
        }

        // Håndterer innsending av spillkode
        [HttpPost]
        public async Task<IActionResult> AddNewGame(EnterCodeViewModel vm)
        {
            // Sjekker om feltet er fylt ut
            if (!ModelState.IsValid)
                return View(vm);

            // Sjekker om koden finnes i databasen
            var story = await _stories.GetPrivateStoryByCode(vm.Code);

            // Hvis koden ikke finnes, vis feilmelding
            if (story == null)
            {
                ModelState.AddModelError(nameof(vm.Code), "Ugyldig kode");
                return View(vm);
            }

            // Hvis koden er gyldig, gå videre til PlayController
            return RedirectToAction("StartByCode", "Play", new { code = vm.Code });
        }

        // Viser logg ut-side (Views/Home/Logout.cshtml)
        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }
    }
}
