using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Jam.Models.Enums;

namespace Jam.ViewModels;

public class CreateStoryAndIntroViewModel
{
    public string Title { get; set; } = string.Empty; // Tittel for intro siden 
    public string Description { get; set; } = string.Empty; //description for intro siden
    public DifficultyLevel DifficultyLevel { get; set; }// vanskelighetsgrad på intro siden som skal vises
    public Accessibility Accessibility { get; set; } // tilgjengelighetsvalg på intro siden
    public string IntroText { get; set; } = string.Empty; // intro tekst for intro siden
    public List<SelectListItem> DifficultyLevelOptions { get; set; } = new(); //verdien som blit valgt for vanskelighetsgrad
    public List<SelectListItem> AccessibilityOptions { get; set; } = new();//Verdien som blit valgt for tilgjengelighetsvalg    
}