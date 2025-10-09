using Microsoft.AspNetCore.Mvc;       
using Microsoft.EntityFrameworkCore;  
using Jam.Data;                       
using Jam.ViewModels;                 

namespace Jam.Controllers
{
    public class HomeController : Controller
    {
        // Felt som holder på database-tilkoblingen (DbContext)
        private readonly GameDbContext _db; //GameDbContext er typen til variabelen db

        public HomeController(GameDbContext db)// Konstruktør 
        {
            _db = db; // lagrer databasen i feltet _db, slik at den kan brukes i alle metoder
        }

        // Viser forsiden til applikasjonen (GET: /Home/Index)
        // IActionResult - metoden returnerer et resultat, vanligvis en View (HTML-side)
        public Task<IActionResult> Index()
        {
            return View(); // returnerer Views/Home/Index.cshtml
        }

        // Viser skjemaet der brukeren kan skrive inn en ID koden (GET: /Home/AddNewGame)
        [HttpGet] 
        public async Task<IActionResult> AddNewGame()
        {
            var viewModel = new EnterCodeVM(); // oppretter et tomt ViewModel-objekt for skjemaet
            return View(viewModel); // sender modellen til visningen (Views/Home/AddNewGame.cshtml)
        }

        // Mottar og håndterer skjemaet etter at brukeren har skrevet inn en spillkode (POST: /Home/AddNewGame)
        [HttpPost]
        public async Task<IActionResult> AddNewGame(EnterCodeVM vm)
        {
            // Sjekker om dataen brukeren sendte inn (vm) er gyldig i forhold til valideringsregler (f.eks. [Required])
            if (!ModelState.IsValid)
                return View(vm); // hvis ugyldig: vis skjemaet igjen med feilmeldinger

            // Søker i databasen etter et Game der "Code" i databasen matcher "Code" som brukeren skrev inn
            // AsNoTracking() = raskere fordi vi ikke skal oppdatere objektet (bare lese)
            // FirstOrDefaultAsync() = finn første rad som passer, eller null hvis ingen finnes
            var game = await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Code == vm.Code); 

            // Hvis ingen spill med den koden ble funnet
            if (game == null)
            {
                // Legger til en feilmelding knyttet til feltet "Code" i ViewModel
                ModelState.AddModelError(nameof(vm.Code), "Ugyldig kode");
                // Viser skjemaet igjen med feilmelding
                return View(vm);
            }

            // Hvis koden var gyldig → send brukeren videre til PlayController.StartByCode
            // "code" sendes som parameter i URL (for eksempel /Play/StartByCode?code=ABC123)
            return RedirectToAction("StartByCode", "Play", new { code = vm.Code });
        }

        // Viser en enkel logg ut-side (GET: /Home/Logout)
        [HttpGet]
        public Task<IActionResult> Logout() => View(); // viser Views/Home/Logout.cshtml
    }
}
