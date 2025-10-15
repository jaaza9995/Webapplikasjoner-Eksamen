using System.ComponentModel.DataAnnotations;
using Jam.Models;

namespace Jam.ViewModels
{
    public class EnterCodeViewModel
    {
        [Required(ErrorMessage = "Skriv inn en kode for å starte spillet")]
        [Display(Name = "Spillkode")]
        public string Code { get; set; } = string.Empty;
    }
}
