using System.ComponentModel.DataAnnotations;

namespace Jam.ViewModels
{
    public class EnterCodeViewModel
    {
        [Required(ErrorMessage = "Skriv inn en kode for å starte spillet")]// Krever at brukeren fyller inn en kode –> viser feilmelding hvis feltet er tomt
        [Display(Name = "Spillkode")]// Bestemmer hvordan navnet på feltet vises i viewet (etikett)
        public string Code { get; set; } = string.Empty; // unik spillkode som brukes for å finne og starte et privat spill
    }

    public class PrivateGameViewModel{
        public string Code { get; set; } = string.Empty;      // koden som ble søkt
        public IEnumerable<StoryListViewModel> Stories { get; set; } 
            = Array.Empty<StoryListViewModel>();              // 0 eller 1 kort
        public string? Error { get; set; }      
    }
}
