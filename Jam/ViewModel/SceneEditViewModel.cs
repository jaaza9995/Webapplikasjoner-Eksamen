using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Jam.Models.Enums;

namespace Jam.ViewModels;

public class SceneEditViewModel
{
    [Required]
    public int SceneId { get; set; }// Unik ID for scenen (brukes ved redigering av eksisterende scene)

    [Required]
    public int StoryId { get; set; }// Forteller hvilken Story (historie) scenen tilhører

    [Required]
    public string Title { get; set; } = string.Empty;// Tittel som vises øverst i scenen

    [Required]
    public SceneType SceneType { get; set; }// Hvilken type scene dette er (Intro, Question, Ending)

    [ValidateNever] 
    public List<SelectListItem> SceneTypeOptions { get; set; } = new();
    // Liste brukt til dropdown i viewet slik at man kan velge SceneType
    // ValidateNever = unngår valideringsfeil siden denne lista ikke sendes tilbake fra skjemaet


    [Required]
    public DifficultyLevel Level { get; set; }// Vanskelighetsgrad (Easy, Medium, Hard)

    [ValidateNever] 
    public List<SelectListItem> LevelOptions { get; set; } = new();
    // Dropdown-valg for vanskelighetsnivå, bygges i controlleren

     // Brukes til å lenke scener sammen i historien
    public int? PreviousSceneId { get; set; } // ID til scenen som kommer før
    public int? NextSceneId { get; set; }     // ID til scenen som kommer etter

    [ValidateNever] public List<SelectListItem> PreviousSceneOptions { get; set; } = new();// Dropdown-liste for valg av forrige scene i editoren
    [ValidateNever] public List<SelectListItem> NextSceneOptions { get; set; } = new();// Dropdown-liste for valg av neste scene i editoren

    // Ulike tekstfelt avhengig av scenetype (controlleren bestemmer hvilket som brukes)
    public string? IntroText { get; set; }     // Teksten som vises for introduksjonsscene
    public string? QuestionText { get; set; }  // Spørsmålstekst for spørsmålsscene
    public string? EndingText { get; set; }    // Tekst som vises for sluttscene


    // Kun for spørsmålsscener
    public List<AnswerOptionEditViewModel> Answers { get; set; } = new();
}

