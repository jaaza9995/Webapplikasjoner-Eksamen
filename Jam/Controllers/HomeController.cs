using Microsoft.AspNetCore.Mvc;


namespace Jam.Controllers
{
    // HomeController håndterer hovedsiden (forsiden) i applikasjonen og generelle visninger som ikke er knyttet til selve spillet.
    public class HomeController : Controller
    {

        public IActionResult Index() // Viser startsiden (Views/Home/Index.cshtml)
        {
            return View();
        }
    }
}
