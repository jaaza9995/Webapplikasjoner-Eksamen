using Microsoft.AspNetCore.Mvc;

namespace Jam.Controllers
{
    // Denne kontrolleren håndterer alt som hører til forsiden og generelle sider i appen
    public class HomeController : Controller
    {
        // Standard handling: viser forsiden
        public IActionResult Index()
        {
            // Returnerer visningen "Views/Home/Index.cshtml"
            return View ();
        } 
    }
}
